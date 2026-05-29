#!/usr/bin/env bash
set -euo pipefail
VPS="widget-vps"
VPS_APP_DIR="/home/k_yamakawa/ops/webforms-migration"
PUBLISH_DIR="./publish/api"

echo "==> [1/4] .NET publish"
dotnet publish src/Api/AttendanceApi.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -o "$PUBLISH_DIR"

echo "==> [2/4] React build"
(cd src/Web && npm ci && npm run build)

echo "==> [3/4] rsync API"
rsync -az --delete \
  -e "ssh -o ClearAllForwardings=yes" \
  --exclude='appsettings*.json' \
  --exclude='wwwroot/' \
  "$PUBLISH_DIR/" \
  "$VPS:$VPS_APP_DIR/"

echo "==> [4/4] rsync frontend"
rsync -az --delete \
  -e "ssh -o ClearAllForwardings=yes" \
  src/Web/dist/ \
  "$VPS:$VPS_APP_DIR/wwwroot/"

echo ""
echo "==> restart"
ssh -o ClearAllForwardings=yes "$VPS" "
  if systemctl is-active --quiet webforms-migration.service; then
    sudo systemctl restart webforms-migration.service
    echo '  webforms-migration.service: restarted'
  else
    echo '  webforms-migration.service: inactive - skipped'
  fi
"

echo "==> done"
