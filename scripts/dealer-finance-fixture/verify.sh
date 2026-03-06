#!/usr/bin/env bash
set -euo pipefail

CONTAINER_NAME="${DEALER_FIXTURE_DB_CONTAINER:-nopcommerce_postgres_server}"
DB_NAME="${DEALER_FIXTURE_DB_NAME:-supplier}"
DB_USER="${DEALER_FIXTURE_DB_USER:-postgres}"

query() {
  docker exec "${CONTAINER_NAME}" psql -U "${DB_USER}" -d "${DB_NAME}" -tAqc "$1"
}

require_eq() {
  local actual="$1"
  local expected="$2"
  local message="$3"

  if [[ "$actual" != "$expected" ]]; then
    echo "FAIL: ${message} (expected=${expected}, actual=${actual})" >&2
    exit 1
  fi

  echo "OK: ${message} => ${actual}"
}

if ! docker ps --format '{{.Names}}' | grep -qx "${CONTAINER_NAME}"; then
  echo "Postgres container not running: ${CONTAINER_NAME}" >&2
  exit 1
fi

require_eq "$(query "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='public' AND table_name='DealerCollection';")" "1" "DealerCollection table exists"
require_eq "$(query "SELECT COUNT(*) FROM \"Store\" WHERE \"Id\" IN (1,2,3) AND \"Deleted\" = FALSE;")" "3" "fixture stores exist"
require_eq "$(query "SELECT COUNT(*) FROM \"Customer\" WHERE \"Email\" IN ('owner-a@test.local','owner-b@test.local','buyer-a1@example.com','buyer-b1@example.com') AND \"Deleted\" = FALSE AND \"Active\" = TRUE;")" "4" "fixture customers active"
require_eq "$(query "SELECT COUNT(*) FROM \"CustomerPassword\" cp JOIN \"Customer\" c ON c.\"Id\" = cp.\"CustomerId\" WHERE c.\"Email\" IN ('owner-a@test.local','owner-b@test.local','buyer-a1@example.com','buyer-b1@example.com') AND cp.\"PasswordFormatId\" = 0 AND cp.\"Password\" = 'Test123!';")" "4" "fixture passwords reset"
require_eq "$(query "SELECT COUNT(*) FROM \"Customer_CustomerRole_Mapping\" m JOIN \"Customer\" c ON c.\"Id\" = m.\"Customer_Id\" JOIN \"CustomerRole\" r ON r.\"Id\" = m.\"CustomerRole_Id\" WHERE c.\"Email\" IN ('owner-a@test.local','owner-b@test.local') AND r.\"SystemName\" = 'StoreOwners';")" "2" "store owner roles assigned"
require_eq "$(query "SELECT COUNT(*) FROM \"DealerInfo\" WHERE \"Id\" IN (1,2) AND \"Active\" = TRUE;")" "2" "fixture dealers active"
require_eq "$(query "SELECT COUNT(*) FROM \"DealerTransaction\";")" "0" "dealer transactions cleared"
require_eq "$(query "SELECT COUNT(*) FROM \"DealerCollection\";")" "0" "dealer collections cleared"
require_eq "$(query "SELECT COUNT(*) FROM \"DealerCustomerMapping\" WHERE (\"DealerId\" = 1 AND \"CustomerId\" IN (SELECT \"Id\" FROM \"Customer\" WHERE \"Email\" IN ('owner-b@test.local','buyer-b1@example.com'))) OR (\"DealerId\" = 2 AND \"CustomerId\" IN (SELECT \"Id\" FROM \"Customer\" WHERE \"Email\" IN ('owner-a@test.local','buyer-a1@example.com')));")" "4" "dealer-customer mappings reset"
require_eq "$(query "SELECT COUNT(*) FROM \"DealerPaymentMethodMapping\" WHERE \"DealerId\" = 2 AND \"PaymentMethodSystemName\" IN ('Payments.CheckMoneyOrder','Payments.OpenAccount');")" "2" "dealer A payment methods reset"
require_eq "$(query "SELECT \"CreditLimit\"::text FROM \"DealerFinancialProfile\" WHERE \"DealerId\" = 2;")" "1000.0000" "dealer A credit limit"
require_eq "$(query "SELECT COUNT(*) FROM \"ShoppingCartItem\" WHERE \"CustomerId\" IN (SELECT \"Id\" FROM \"Customer\" WHERE \"Email\" IN ('owner-a@test.local','owner-b@test.local','buyer-a1@example.com','buyer-b1@example.com'));")" "0" "fixture carts cleared"
require_eq "$(query "SELECT \"Value\" FROM \"Setting\" WHERE \"Name\" = 'paymentsettings.activepaymentmethodsystemnames' LIMIT 1;")" "Payments.CheckMoneyOrder,Payments.Manual,Payments.OpenAccount" "active payment methods"
require_eq "$(query "SELECT \"Price\"::text FROM \"Product\" WHERE \"Id\" = 48;")" "200.0000" "fixture product A price"
require_eq "$(query "SELECT COUNT(*) FROM \"StoreMapping\" WHERE \"EntityName\" = 'Product' AND ((\"EntityId\" = 48 AND \"StoreId\" = 2) OR (\"EntityId\" = 49 AND \"StoreId\" = 1));")" "2" "fixture product store mappings"

echo "Fixture verification completed successfully."
