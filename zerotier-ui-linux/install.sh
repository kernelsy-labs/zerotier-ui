#!/usr/bin/env bash
set -euo pipefail
[[ $EUID -eq 0 ]] || { echo "Run: sudo ./install.sh"; exit 1; }
ROOT="$(cd "$(dirname "$0")" && pwd)"
if command -v pacman >/dev/null; then pacman -Sy --needed --noconfirm zerotier-one python-gobject gtk4
elif command -v apt-get >/dev/null; then apt-get update; apt-get install -y python3 python3-gi gir1.2-gtk-4.0 zerotier-one
elif command -v dnf >/dev/null; then dnf install -y python3 python3-gobject gtk4 zerotier-one
else echo "Unsupported distro"; exit 1; fi
systemctl enable --now zerotier-one 2>/dev/null || true
install -Dm755 "$ROOT/zerotier_ui.py" /usr/local/lib/zerotier-ui/zerotier_ui.py
install -Dm644 "$ROOT/io.github.kernelsyLabs.ZeroTierUI.desktop" /usr/share/applications/io.github.kernelsyLabs.ZeroTierUI.desktop
cat > /usr/local/bin/zerotier-ui <<'EOF'
#!/bin/sh
exec python3 /usr/local/lib/zerotier-ui/zerotier_ui.py "$@"
EOF
chmod 755 /usr/local/bin/zerotier-ui
echo "Installed. Launch from menu or run: zerotier-ui"
