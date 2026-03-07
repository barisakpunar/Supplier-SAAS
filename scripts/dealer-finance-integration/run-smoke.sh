#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
FIXTURE_APPLY="$ROOT_DIR/scripts/dealer-finance-fixture/apply.sh"
FIXTURE_VERIFY="$ROOT_DIR/scripts/dealer-finance-fixture/verify.sh"
APP_URL="${APP_URL:-http://localhost:5110}"
LOGIN_URL="$APP_URL/login?returnUrl=%2Fadmin"
LOGIN_POST_URL="$APP_URL/login?returnurl=%2Fadmin"
COOKIE_JAR="${TMPDIR:-/tmp}/dealer-finance-smoke.cookies"
LOGIN_HTML="${TMPDIR:-/tmp}/dealer-finance-login.html"
LOGIN_HEADERS="${TMPDIR:-/tmp}/dealer-finance-login.headers"
LOGIN_BODY="${TMPDIR:-/tmp}/dealer-finance-login.body"
DB_CONTAINER="${DB_CONTAINER:-nopcommerce_postgres_server}"
DB_NAME="${DB_NAME:-supplier}"
DB_USER="${DB_USER:-postgres}"

info() {
  printf '[dealer-finance-smoke] %s\n' "$1"
}

fail() {
  printf '[dealer-finance-smoke] ERROR: %s\n' "$1" >&2
  exit 1
}

assert_contains() {
  local haystack="$1"
  local needle="$2"
  local message="$3"
  if [[ "$haystack" != *"$needle"* ]]; then
    fail "$message"
  fi
}

assert_equals() {
  local actual="$1"
  local expected="$2"
  local message="$3"
  if [[ "$actual" != "$expected" ]]; then
    fail "$message (actual=$actual expected=$expected)"
  fi
}

require_cmd() {
  command -v "$1" >/dev/null 2>&1 || fail "Required command not found: $1"
}

require_cmd curl
require_cmd docker
require_cmd perl

info "Applying deterministic finance fixture"
bash "$FIXTURE_APPLY"

info "Verifying deterministic finance fixture"
bash "$FIXTURE_VERIFY"

info "Checking application login page"
curl -fsS -c "$COOKIE_JAR" "$LOGIN_URL" > "$LOGIN_HTML"
token="$(grep -o 'name="__RequestVerificationToken" type="hidden" value="[^"]*' "$LOGIN_HTML" | head -n1 | sed 's/.*value="//')"
[[ -n "$token" ]] || fail "Request verification token could not be parsed from login page"

info "Submitting store owner login"
curl -fsS -D "$LOGIN_HEADERS" -o "$LOGIN_BODY" \
  -b "$COOKIE_JAR" -c "$COOKIE_JAR" \
  -X POST "$LOGIN_POST_URL" \
  --data-urlencode "Email=owner-a@test.local" \
  --data-urlencode "Password=Test123!" \
  --data-urlencode "RememberMe=false" \
  --data-urlencode "__RequestVerificationToken=$token"

login_headers="$(cat "$LOGIN_HEADERS")"
assert_contains "$login_headers" "HTTP/1.1 302" "Store owner login did not return HTTP 302"
assert_contains "$login_headers" "Location: /admin" "Store owner login did not redirect to /admin"
assert_contains "$login_headers" ".Nop.Authentication=" "Store owner login did not issue auth cookie"

info "Running PostgreSQL assertions"
SQL_QUERY="$(cat <<'SQL'
SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='public' AND table_name='DealerCollection';
SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='public' AND table_name='DealerFinancialInstrument';
SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='public' AND table_name='DealerTransactionAllocation';
SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='public' AND table_name='DealerFinanceAuditLog';
SELECT COUNT(*) FROM "DealerInfo" WHERE "Id" = 2 AND "Name" = 'Dealer A' AND "StoreId" = 2;
SELECT COUNT(*) FROM "DealerFinancialProfile" WHERE "DealerId" = 2 AND "OpenAccountEnabled" = TRUE AND "CreditLimit" = 1000.0000;
SELECT COUNT(*) FROM "Customer" WHERE "Email" = 'owner-a@test.local';
SELECT COUNT(*) FROM "Customer" WHERE "Email" = 'buyer-a1@example.com';
SELECT COUNT(*) FROM "DealerCustomerMapping" WHERE "DealerId" = 2 AND "CustomerId" IN (11, 26);
SELECT COUNT(*) FROM "DealerPaymentMethodMapping" WHERE "DealerId" = 2 AND "PaymentMethodSystemName" = 'Payments.OpenAccount';
SELECT COUNT(*) FROM "LocaleStringResource" WHERE "ResourceName" LIKE 'Admin.Customers.DealerCollections.Allocations%';
SELECT COUNT(*) FROM "LocaleStringResource" WHERE "ResourceName" IN ('Admin.Customers.DealerFinanceAudit.ActionType.CollectionAllocationCreated','Admin.Customers.DealerFinanceAudit.ActionType.CollectionAllocationCancelled');
SQL
)"
db_checks_raw="$(docker exec "$DB_CONTAINER" psql -U "$DB_USER" -d "$DB_NAME" -tAc "$SQL_QUERY")"
db_checks=()
while IFS= read -r line; do
  db_checks+=("$line")
done <<< "$db_checks_raw"

assert_equals "${db_checks[0]// /}" "1" "DealerCollection table is missing"
assert_equals "${db_checks[1]// /}" "1" "DealerFinancialInstrument table is missing"
assert_equals "${db_checks[2]// /}" "1" "DealerTransactionAllocation table is missing"
assert_equals "${db_checks[3]// /}" "1" "DealerFinanceAuditLog table is missing"
assert_equals "${db_checks[4]// /}" "1" "Dealer A fixture record is missing"
assert_equals "${db_checks[5]// /}" "1" "Dealer A financial profile fixture is missing"
assert_equals "${db_checks[6]// /}" "1" "owner-a@test.local fixture customer is missing"
assert_equals "${db_checks[7]// /}" "1" "buyer-a1@example.com fixture customer is missing"
assert_equals "${db_checks[8]// /}" "2" "Dealer A customer mapping fixture is incomplete"
assert_equals "${db_checks[9]// /}" "1" "Dealer A open account payment mapping is missing"

if [[ "${db_checks[10]// /}" -lt 1 ]]; then
  fail "Dealer collection allocation localization resources are missing"
fi

assert_equals "${db_checks[11]// /}" "4" "Allocation audit localization resources are missing"

info "Smoke checks passed"
printf 'OK: PostgreSQL fixture, login, and finance schema checks passed.\n'
