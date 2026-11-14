# ZeroTier Quick Toggle Linux

Aplikasi terminal untuk mengelola network ZeroTier tanpa membuka tray menu.

## Install

```bash
chmod +x install.sh
sudo ./install.sh
```

Installer mendeteksi Arch, Debian/Ubuntu, atau Fedora dan memasang `zerotier-one` bila tersedia di repository distro.

Jalankan:

```bash
zttoggle
```

Perintah:

- `list` — tampilkan semua network dan status
- `enable <ID>` — join/aktifkan network
- `disable <ID>` — leave/nonaktifkan network
- `add <ID> [nama]` — simpan ID agar tetap muncul meski sedang disabled
- `refresh` — reload daftar
- `help` — bantuan
- `quit` — keluar

File ID tersimpan di `~/.config/zerotier-quick-toggle/networks`.
