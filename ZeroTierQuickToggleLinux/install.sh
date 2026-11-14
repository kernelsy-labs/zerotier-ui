#!/usr/bin/env bash
set -euo pipefail

[[ $EUID -eq 0 ]] || { echo "Jalankan: sudo ./install.sh"; exit 1; }
ROOT="$(cd "$(dirname "$0")" && pwd)"

if command -v pacman >/dev/null; then
  pacman -Sy --needed --noconfirm zerotier-one python
elif command -v apt-get >/dev/null; then
  apt-get update
  apt-get install -y python3 curl
  if ! command -v zerotier-cli >/dev/null; then curl -s https://install.zerotier.com | bash; fi
elif command -v dnf >/dev/null; then
  dnf install -y python3 curl
  if ! command -v zerotier-cli >/dev/null; then curl -s https://install.zerotier.com | bash; fi
else
  echo "Distro tidak dikenali. Install ZeroTier dan python3 secara manual."; exit 1
fi

systemctl enable --now zerotier-one 2>/dev/null || true
install -Dm755 "$ROOT/zttoggle" /usr/local/bin/zttoggle
echo "Selesai. Jalankan: zttoggle"
