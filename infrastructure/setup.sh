#!/usr/bin/env bash
# VPS 初回セットアップ: systemd ユニット登録
# 実行場所: VPS 上で直接実行
set -euo pipefail

APP_DIR="/home/k_yamakawa/ops/webforms-migration"
SERVICE="webforms-migration"
USER="k_yamakawa"

echo "==> [1/3] ディレクトリ作成"
mkdir -p "$APP_DIR/wwwroot"

echo "==> [2/3] systemd ユニット登録"
sudo tee /etc/systemd/system/${SERVICE}.service > /dev/null <<EOF
[Unit]
Description=WebForms Migration App (.NET 8)
After=network.target

[Service]
User=${USER}
WorkingDirectory=${APP_DIR}
ExecStart=${APP_DIR}/AttendanceApi
Restart=always
RestartSec=5

[Install]
WantedBy=multi-user.target
EOF

echo "==> [3/3] サービス有効化"
sudo systemctl daemon-reload
sudo systemctl enable ${SERVICE}.service

echo "==> done"
echo ""
echo "=========================================="
echo " 初回起動:"
echo "   sudo systemctl start ${SERVICE}.service"
echo " 状態確認:"
echo "   systemctl status ${SERVICE}.service"
echo "=========================================="
