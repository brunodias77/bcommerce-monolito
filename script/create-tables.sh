#!/usr/bin/env bash
set -euo pipefail
ROOT_DIR="$(cd "$(dirname "$0")"/.. && pwd)"
docker exec -i bcommerce-postgres psql -U bcommerce -d bcommerce_db -v ON_ERROR_STOP=0 < "$ROOT_DIR/docs/db/schema.sql"

