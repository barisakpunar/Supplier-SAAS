BEGIN;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'DealerCollection') THEN
        RAISE EXCEPTION 'DealerCollection table does not exist. Run the application once on branch codex/test/dealer-finance-fixture so nop migrations can be applied.';
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "Store" WHERE "Name" = 'Main Store' AND "Deleted" = FALSE) THEN
        UPDATE "Store"
        SET "DisplayOrder" = "DisplayOrder" + 1
        WHERE "Deleted" = FALSE;

        INSERT INTO "Store" (
            "Name", "Url", "Hosts", "CompanyName", "CompanyAddress", "CompanyPhoneNumber", "CompanyVat",
            "DefaultMetaKeywords", "DefaultMetaDescription", "DefaultTitle", "HomepageTitle", "HomepageDescription",
            "SslEnabled", "DefaultLanguageId", "DisplayOrder", "Deleted"
        )
        SELECT
            'Main Store', s."Url", '', 'Main Store', '', '', NULL,
            s."DefaultMetaKeywords", s."DefaultMetaDescription", 'Main Store', 'Main Store', 'Main Store',
            s."SslEnabled", s."DefaultLanguageId", 1, FALSE
        FROM "Store" s
        WHERE s."Deleted" = FALSE
        ORDER BY s."DisplayOrder", s."Id"
        LIMIT 1;
    END IF;

    IF EXISTS (SELECT 1 FROM "Store" WHERE "Name" = 'Your store name' AND "Deleted" = FALSE)
       AND NOT EXISTS (SELECT 1 FROM "Store" WHERE "Name" = 'Store B' AND "Deleted" = FALSE) THEN
        UPDATE "Store"
        SET "Name" = 'Store B',
            "DefaultTitle" = 'Store B',
            "HomepageTitle" = 'Store B',
            "HomepageDescription" = 'Store B',
            "CompanyName" = 'Store B'
        WHERE "Name" = 'Your store name'
          AND "Deleted" = FALSE;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM "Store" WHERE "Name" = 'Store B' AND "Deleted" = FALSE) THEN
        INSERT INTO "Store" (
            "Name", "Url", "Hosts", "CompanyName", "CompanyAddress", "CompanyPhoneNumber", "CompanyVat",
            "DefaultMetaKeywords", "DefaultMetaDescription", "DefaultTitle", "HomepageTitle", "HomepageDescription",
            "SslEnabled", "DefaultLanguageId", "DisplayOrder", "Deleted"
        )
        SELECT
            'Store B', s."Url", '', 'Store B', '', '', NULL,
            s."DefaultMetaKeywords", s."DefaultMetaDescription", 'Store B', 'Store B', 'Store B',
            s."SslEnabled", s."DefaultLanguageId", 2, FALSE
        FROM "Store" s
        WHERE s."Deleted" = FALSE
        ORDER BY s."DisplayOrder", s."Id"
        LIMIT 1;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM "Store" WHERE "Name" = 'Store A' AND "Deleted" = FALSE) THEN
        INSERT INTO "Store" (
            "Name", "Url", "Hosts", "CompanyName", "CompanyAddress", "CompanyPhoneNumber", "CompanyVat",
            "DefaultMetaKeywords", "DefaultMetaDescription", "DefaultTitle", "HomepageTitle", "HomepageDescription",
            "SslEnabled", "DefaultLanguageId", "DisplayOrder", "Deleted"
        )
        SELECT
            'Store A', s."Url", '', 'Store A', '', '', NULL,
            s."DefaultMetaKeywords", s."DefaultMetaDescription", 'Store A', 'Store A', 'Store A',
            s."SslEnabled", s."DefaultLanguageId", 3, FALSE
        FROM "Store" s
        WHERE s."Deleted" = FALSE
        ORDER BY s."DisplayOrder", s."Id"
        LIMIT 1;
    END IF;

    UPDATE "Store"
    SET "DisplayOrder" = CASE "Name"
            WHEN 'Main Store' THEN 1
            WHEN 'Store B' THEN 2
            WHEN 'Store A' THEN 3
            ELSE "DisplayOrder"
        END
    WHERE "Name" IN ('Main Store', 'Store B', 'Store A')
      AND "Deleted" = FALSE;
END $$;

-- Normalize global settings required by the finance fixture.
UPDATE "Setting"
SET "Value" = 'False'
WHERE "Name" IN ('catalogsettings.ignorestorelimitations', 'catalogsettings.allowanonymoususerstoreviewproduct');

UPDATE "Setting"
SET "Value" = 'Payments.CheckMoneyOrder,Payments.Manual,Payments.OpenAccount'
WHERE "Name" = 'paymentsettings.activepaymentmethodsystemnames';

INSERT INTO "Customer" (
    "Username", "Email", "CustomerGuid", "CountryId", "StateProvinceId", "VatNumberStatusId",
    "AffiliateId", "VendorId", "IsTaxExempt", "HasShoppingCartItems", "RequireReLogin", "FailedLoginAttempts",
    "Active", "Deleted", "IsSystemAccount", "CreatedOnUtc", "LastActivityDateUtc",
    "RegisteredInStoreId", "MustChangePassword"
)
SELECT
    seed."Email",
    seed."Email",
    seed."CustomerGuid"::uuid,
    0,
    0,
    0,
    0,
    0,
    FALSE,
    FALSE,
    FALSE,
    0,
    TRUE,
    FALSE,
    FALSE,
    NOW(),
    NOW(),
    (SELECT "Id" FROM "Store" WHERE "Name" = seed."StoreName" AND "Deleted" = FALSE ORDER BY "DisplayOrder", "Id" LIMIT 1),
    FALSE
FROM (
    VALUES
        ('owner-a@test.local', 'Store A', '00000000-0000-0000-0000-0000000000a1'),
        ('owner-b@test.local', 'Store B', '00000000-0000-0000-0000-0000000000b1'),
        ('buyer-a1@example.com', 'Store A', '00000000-0000-0000-0000-0000000000a2'),
        ('buyer-b1@example.com', 'Store B', '00000000-0000-0000-0000-0000000000b2')
) AS seed("Email", "StoreName", "CustomerGuid")
WHERE NOT EXISTS (
    SELECT 1
    FROM "Customer" c
    WHERE c."Email" = seed."Email"
);

-- Reset fixture users to deterministic credentials and stores.
UPDATE "Customer"
SET "RegisteredInStoreId" = CASE "Email"
        WHEN 'owner-a@test.local' THEN (SELECT "Id" FROM "Store" WHERE "Name" = 'Store A' AND "Deleted" = FALSE ORDER BY "DisplayOrder", "Id" LIMIT 1)
        WHEN 'owner-b@test.local' THEN (SELECT "Id" FROM "Store" WHERE "Name" = 'Store B' AND "Deleted" = FALSE ORDER BY "DisplayOrder", "Id" LIMIT 1)
        WHEN 'buyer-a1@example.com' THEN (SELECT "Id" FROM "Store" WHERE "Name" = 'Store A' AND "Deleted" = FALSE ORDER BY "DisplayOrder", "Id" LIMIT 1)
        WHEN 'buyer-b1@example.com' THEN (SELECT "Id" FROM "Store" WHERE "Name" = 'Store B' AND "Deleted" = FALSE ORDER BY "DisplayOrder", "Id" LIMIT 1)
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
SET "Deleted" = TRUE,
    "OrderStatusId" = 40,
    "PaymentStatusId" = CASE WHEN "PaymentStatusId" = 0 THEN 10 ELSE "PaymentStatusId" END
WHERE "CustomerId" IN (
    SELECT "Id" FROM "Customer" WHERE "Email" IN ('buyer-a1@example.com', 'buyer-b1@example.com')
)
  AND "PaymentMethodSystemName" = 'Payments.OpenAccount';

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

INSERT INTO "DealerInfo" ("Id", "Name", "StoreId", "Active", "CreatedOnUtc", "UpdatedOnUtc")
VALUES
    (1, 'Dealer B', (SELECT "Id" FROM "Store" WHERE "Name" = 'Store B' AND "Deleted" = FALSE ORDER BY "DisplayOrder", "Id" LIMIT 1), TRUE, NOW(), NOW()),
    (2, 'Dealer A', (SELECT "Id" FROM "Store" WHERE "Name" = 'Store A' AND "Deleted" = FALSE ORDER BY "DisplayOrder", "Id" LIMIT 1), TRUE, NOW(), NOW()),
    (3, 'Unused Dealer', (SELECT "Id" FROM "Store" WHERE "Name" = 'Store A' AND "Deleted" = FALSE ORDER BY "DisplayOrder", "Id" LIMIT 1), FALSE, NOW(), NOW())
ON CONFLICT ("Id") DO UPDATE
SET "Name" = EXCLUDED."Name",
    "StoreId" = EXCLUDED."StoreId",
    "Active" = EXCLUDED."Active",
    "UpdatedOnUtc" = NOW();

SELECT setval('"DealerInfo_Id_seq"', GREATEST((SELECT COALESCE(MAX("Id"), 0) FROM "DealerInfo"), 1), TRUE);

UPDATE "DealerInfo"
SET "Name" = CASE "Id"
        WHEN 1 THEN 'Dealer B'
        WHEN 2 THEN 'Dealer A'
        WHEN 3 THEN 'Unused Dealer'
        ELSE "Name"
    END,
    "StoreId" = CASE "Id"
        WHEN 1 THEN (SELECT "Id" FROM "Store" WHERE "Name" = 'Store B' AND "Deleted" = FALSE ORDER BY "DisplayOrder", "Id" LIMIT 1)
        WHEN 2 THEN (SELECT "Id" FROM "Store" WHERE "Name" = 'Store A' AND "Deleted" = FALSE ORDER BY "DisplayOrder", "Id" LIMIT 1)
        WHEN 3 THEN (SELECT "Id" FROM "Store" WHERE "Name" = 'Store A' AND "Deleted" = FALSE ORDER BY "DisplayOrder", "Id" LIMIT 1)
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
        WHEN 15 THEN 'Fixture Category A'
        WHEN 16 THEN 'Fixture Category B'
        ELSE "Name"
    END,
    "LimitedToStores" = TRUE,
    "Published" = TRUE,
    "Deleted" = FALSE,
    "UpdatedOnUtc" = NOW()
WHERE "Id" IN (15, 16);

DELETE FROM "StoreMapping"
WHERE ("EntityName" = 'Category' AND "EntityId" IN (15, 16))
   OR ("EntityName" = 'Product' AND "EntityId" IN (46, 47));

INSERT INTO "StoreMapping" ("EntityName", "StoreId", "EntityId")
VALUES
    ('Category', (SELECT "Id" FROM "Store" WHERE "Name" = 'Store A' AND "Deleted" = FALSE ORDER BY "DisplayOrder", "Id" LIMIT 1), 15),
    ('Category', (SELECT "Id" FROM "Store" WHERE "Name" = 'Store B' AND "Deleted" = FALSE ORDER BY "DisplayOrder", "Id" LIMIT 1), 16),
    ('Product', (SELECT "Id" FROM "Store" WHERE "Name" = 'Store A' AND "Deleted" = FALSE ORDER BY "DisplayOrder", "Id" LIMIT 1), 46),
    ('Product', (SELECT "Id" FROM "Store" WHERE "Name" = 'Store B' AND "Deleted" = FALSE ORDER BY "DisplayOrder", "Id" LIMIT 1), 47);

UPDATE "Product"
SET "Name" = CASE "Id"
        WHEN 46 THEN 'Fixture Product A'
        WHEN 47 THEN 'Fixture Product B'
        ELSE "Name"
    END,
    "Price" = CASE "Id"
        WHEN 46 THEN 200.0000
        WHEN 47 THEN 150.0000
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
WHERE "Id" IN (46, 47);

UPDATE "Product_Category_Mapping"
SET "CategoryId" = CASE "ProductId"
        WHEN 46 THEN 15
        WHEN 47 THEN 16
        ELSE "CategoryId"
    END,
    "DisplayOrder" = 1,
    "IsFeaturedProduct" = FALSE
WHERE "ProductId" IN (46, 47);

COMMIT;
