# ZeroTier UI for Linux

GTK4 desktop application for quickly enabling and disabling ZeroTier networks.
It uses the official `zerotier-cli` command and keeps disabled network IDs in a local config file, so they remain visible after `leave`.

## Features

- GTK4 graphical interface for Arch Linux, Debian/Ubuntu, and Fedora
- List network ID, name, and current status
- **Enable** runs `zerotier-cli join NETWORK_ID`
- **Disable** runs `zerotier-cli leave NETWORK_ID`
- Add a network ID manually so it remains visible when disabled
- Refresh status and open the ZeroTier network manager
- No tokens or credentials are stored by this application

## Requirements

- Linux with GTK4 and PyGObject
- ZeroTier One installed and its service running
- User permission to run `zerotier-cli` (installer configures a small sudo rule for the CLI)

## Install

```bash
chmod +x install.sh
sudo ./install.sh
```

The installer detects:

- Arch Linux: `pacman`
- Debian/Ubuntu: `apt`
- Fedora: `dnf`

Start it from the application menu as **ZeroTier UI**, or run:

```bash
zerotier-ui
```

## Manual run

```bash
python3 zerotier_ui.py
```

## Stored data

Network IDs and display names are stored at:

```text
~/.config/zerotier-ui/networks.json
```

The file contains only network IDs and names. It does not contain the ZeroTier authentication token.

## Package distribution

The Git repository is enough for source distribution. A package is recommended later for end users:

- Arch: PKGBUILD/AUR
- Debian/Ubuntu: `.deb`
- Fedora: `.rpm`

The current installer is intentionally simple and supports all three families from one checkout.

## License

MIT License. See `LICENSE`.
