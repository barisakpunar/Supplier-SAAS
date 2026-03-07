BEGIN;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'DealerCollection') THEN
        RAISE EXCEPTION 'DealerCollection table does not exist. Run the application once on branch codex/test/dealer-finance-fixture so nop migrations can be applied.';
    END IF;
END $$;

-- Normalize global settings required by the finance fixture.
UPDATE "Setting"
SET "Value" = 'False'
WHERE "Name" IN ('catalogsettings.ignorestorelimitations', 'catalogsettings.allowanonymoususerstoreviewproduct');

UPDATE "Setting"
SET "Value" = 'Payments.CheckMoneyOrder,Payments.Manual,Payments.OpenAccount'
WHERE "Name" = 'paymentsettings.activepaymentmethodsystemnames';

-- Reset fixture users to deterministic credentials and stores.
UPDATE "Customer"
SET "RegisteredInStoreId" = CASE "Email"
        WHEN 'owner-a@test.local' THEN 2
        WHEN 'owner-b@test.local' THEN 1
        WHEN 'buyer-a1@example.com' THEN 2
        WHEN 'buyer-b1@example.com' THEN 1
        ELSE "RegisteredInStoreId"
    END,
    "Active" = TRUE,
    "Deleted" = FALSE,
    "IsSystemAccount" = FALSE,
    "HasShoppingCartItems" = FALSE,
    "RequireReLogin" = FALSE,
    "FailedLoginAttempts" = 0,
    "CannotLoginUntilDateUtc" = NULL,
    "LastActivityDateUtc" = NOW(),
    "MustChangePassword" = FALSE
WHERE "Email" IN ('owner-a@test.local', 'owner-b@test.local', 'buyer-a1@example.com', 'buyer-b1@example.com');

DELETE FROM "CustomerPassword"
WHERE "CustomerId" IN (
    SELECT "Id" FROM "Customer"
    WHERE "Email" IN ('owner-a@test.local', 'owner-b@test.local', 'buyer-a1@example.com', 'buyer-b1@example.com')
);

INSERT INTO "CustomerPassword" ("CustomerId", "Password", "PasswordFormatId", "PasswordSalt", "CreatedOnUtc")
SELECT "Id", 'Test123!', 0, NULL, NOW()
FROM "Customer"
WHERE "Email" IN ('owner-a@test.local', 'owner-b@test.local', 'buyer-a1@example.com', 'buyer-b1@example.com');

DELETE FROM "Customer_CustomerRole_Mapping"
WHERE "Customer_Id" IN (
    SELECT "Id" FROM "Customer"
    WHERE "Email" IN ('owner-a@test.local', 'owner-b@test.local', 'buyer-a1@example.com', 'buyer-b1@example.com')
);

INSERT INTO "Customer_CustomerRole_Mapping" ("Customer_Id", "CustomerRole_Id")
SELECT c."Id", r."Id"
FROM "Customer" c
JOIN "CustomerRole" r ON r."SystemName" = 'Registered'
WHERE c."Email" IN ('owner-a@test.local', 'owner-b@test.local', 'buyer-a1@example.com', 'buyer-b1@example.com');

INSERT INTO "Customer_CustomerRole_Mapping" ("Customer_Id", "CustomerRole_Id")
SELECT c."Id", r."Id"
FROM "Customer" c
JOIN "CustomerRole" r ON r."SystemName" = 'StoreOwners'
WHERE c."Email" IN ('owner-a@test.local', 'owner-b@test.local');

DELETE FROM "ShoppingCartItem"
WHERE "CustomerId" IN (
    SELECT "Id" FROM "Customer"
    WHERE "Email" IN ('owner-a@test.local', 'owner-b@test.local', 'buyer-a1@example.com', 'buyer-b1@example.com')
);

DELETE FROM "GenericAttribute"
WHERE "KeyGroup" = 'Customer'
  AND "EntityId" IN (
      SELECT "Id" FROM "Customer"
      WHERE "Email" IN ('owner-a@test.local', 'owner-b@test.local', 'buyer-a1@example.com', 'buyer-b1@example.com')
  );

-- Neutralize previous open-account history for target buyers so debt starts from zero.
UPDATE "Order"
SET "PaymentStatusId" = 30,
    "OrderStatusId" = CASE WHEN "OrderStatusId" < 20 THEN 20 ELSE "OrderStatusId" END,
    "PaidDateUtc" = COALESCE("PaidDateUtc", NOW())
WHERE "CustomerId" IN (
    SELECT "Id" FROM "Customer" WHERE "Email" IN ('buyer-a1@example.com', 'buyer-b1@example.com')
)
  AND "PaymentMethodSystemName" = 'Payments.OpenAccount'
  AND "PaymentStatusId" IN (10, 20);

-- Reset dealer finance state.
DELETE FROM "DealerFinanceAuditLog";
DELETE FROM "DealerTransactionAllocation";
UPDATE "DealerCollection"
SET "DealerFinancialInstrumentId" = NULL,
    "DealerTransactionId" = NULL,
    "CancelledDealerTransactionId" = NULL;
UPDATE "DealerFinancialInstrument"
SET "DealerCollectionId" = NULL;
DELETE FROM "DealerFinancialInstrument";
DELETE FROM "DealerCollection";
DELETE FROM "DealerTransaction";

UPDATE "DealerInfo"
SET "Name" = CASE "Id"
        WHEN 1 THEN 'Dealer B'
        WHEN 2 THEN 'Dealer A'
        WHEN 3 THEN 'Unused Dealer'
        ELSE "Name"
    END,
    "StoreId" = CASE "Id"
        WHEN 1 THEN 1
        WHEN 2 THEN 2
        WHEN 3 THEN 2
        ELSE "StoreId"
    END,
    "Active" = CASE WHEN "Id" IN (1, 2) THEN TRUE ELSE FALSE END,
    "UpdatedOnUtc" = NOW()
WHERE "Id" IN (1, 2, 3);

DELETE FROM "DealerCustomerMapping"
WHERE "DealerId" IN (1, 2, 3);

INSERT INTO "DealerCustomerMapping" ("DealerId", "CustomerId")
SELECT 1, c."Id"
FROM "Customer" c
WHERE c."Email" IN ('owner-b@test.local', 'buyer-b1@example.com');

INSERT INTO "DealerCustomerMapping" ("DealerId", "CustomerId")
SELECT 2, c."Id"
FROM "Customer" c
WHERE c."Email" IN ('owner-a@test.local', 'buyer-a1@example.com');

DELETE FROM "DealerFinancialProfile"
WHERE "DealerId" IN (1, 2, 3);

INSERT INTO "DealerFinancialProfile" ("DealerId", "OpenAccountEnabled", "CreditLimit", "CreatedOnUtc", "UpdatedOnUtc")
VALUES
    (1, FALSE, 0.0000, NOW(), NOW()),
    (2, TRUE, 1000.0000, NOW(), NOW()),
    (3, FALSE, 0.0000, NOW(), NOW());

DELETE FROM "DealerPaymentMethodMapping"
WHERE "DealerId" IN (1, 2, 3);

INSERT INTO "DealerPaymentMethodMapping" ("DealerId", "PaymentMethodSystemName")
VALUES
    (1, 'Payments.CheckMoneyOrder'),
    (2, 'Payments.CheckMoneyOrder'),
    (2, 'Payments.OpenAccount');

-- Stabilize store-specific catalog items used in checkout tests.
UPDATE "Category"
SET "Name" = CASE "Id"
        WHEN 17 THEN 'Fixture Category A'
        WHEN 18 THEN 'Fixture Category B'
        ELSE "Name"
    END,
    "LimitedToStores" = TRUE,
    "Published" = TRUE,
    "Deleted" = FALSE,
    "UpdatedOnUtc" = NOW()
WHERE "Id" IN (17, 18);

DELETE FROM "StoreMapping"
WHERE ("EntityName" = 'Category' AND "EntityId" IN (17, 18))
   OR ("EntityName" = 'Product' AND "EntityId" IN (48, 49));

INSERT INTO "StoreMapping" ("EntityName", "StoreId", "EntityId")
VALUES
    ('Category', 2, 17),
    ('Category', 1, 18),
    ('Product', 2, 48),
    ('Product', 1, 49);

UPDATE "Product"
SET "Name" = CASE "Id"
        WHEN 48 THEN 'Fixture Product A'
        WHEN 49 THEN 'Fixture Product B'
        ELSE "Name"
    END,
    "Price" = CASE "Id"
        WHEN 48 THEN 200.0000
        WHEN 49 THEN 150.0000
        ELSE "Price"
    END,
    "LimitedToStores" = TRUE,
    "Published" = TRUE,
    "Deleted" = FALSE,
    "VisibleIndividually" = TRUE,
    "DisableBuyButton" = FALSE,
    "RequireOtherProducts" = FALSE,
    "OrderMinimumQuantity" = 1,
    "OrderMaximumQuantity" = 10000,
    "StockQuantity" = 10000,
    "UpdatedOnUtc" = NOW()
WHERE "Id" IN (48, 49);

UPDATE "Product_Category_Mapping"
SET "CategoryId" = CASE "ProductId"
        WHEN 48 THEN 17
        WHEN 49 THEN 18
        ELSE "CategoryId"
    END,
    "DisplayOrder" = 1,
    "IsFeaturedProduct" = FALSE
WHERE "ProductId" IN (48, 49);

COMMIT;
