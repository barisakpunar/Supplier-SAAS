#!/usr/bin/env bash
set -euo pipefail

CONTAINER_NAME="${DEALER_FIXTURE_DB_CONTAINER:-nopcommerce_postgres_server}"
DB_NAME="${DEALER_FIXTURE_DB_NAME:-supplier}"
DB_USER="${DEALER_FIXTURE_DB_USER:-postgres}"
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
SQL_FILE="${SCRIPT_DIR}/fixture.sql"

if ! docker ps --format '{{.Names}}' | grep -qx "${CONTAINER_NAME}"; then
  echo "Postgres container not running: ${CONTAINER_NAME}" >&2
  exit 1
fi

if ! docker exec "${CONTAINER_NAME}" psql -U "${DB_USER}" -d "${DB_NAME}" -tAc "SELECT 1 FROM information_schema.tables WHERE table_schema='public' AND table_name='DealerCollection';" | grep -qx '1'; then
  echo "DealerCollection table is missing in ${DB_NAME}. Start the nopCommerce app once on branch codex/test/dealer-finance-fixture so migrations can run." >&2
  exit 1
fi

echo "Applying dealer finance fixture to ${DB_NAME} on ${CONTAINER_NAME}..."
docker exec -i "${CONTAINER_NAME}" psql -v ON_ERROR_STOP=1 -U "${DB_USER}" -d "${DB_NAME}" < "${SQL_FILE}"
echo "Fixture applied."
