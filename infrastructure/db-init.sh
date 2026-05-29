#!/usr/bin/env bash
set -euo pipefail

VPS="widget-vps"

echo "==> DB schema apply"
ssh "$VPS" "PGPASSWORD=kintai_pass psql -h 127.0.0.1 -U kintai_user -d KINTAI" \
  < infrastructure/db/init/01_schema.sql
echo "==> done"
