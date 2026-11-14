# ZeroTier UI

Cross-platform helper for quickly enabling and disabling ZeroTier networks.

- Windows: `ZeroTierQuickToggle/` (WinForms .NET 8)
- Linux: `zerotier-ui-linux/` (GTK4/PyGObject)

## Windows distribution

The Windows project can be used as a portable single EXE or installed with the Inno Setup installer. GitHub Actions builds both artifacts automatically when you run the workflow manually or push a tag such as `v1.0.0`.

To create a release:

```bash
git tag v1.0.0
git push origin v1.0.0
```

The release contains `ZeroTierQuickToggle-Setup.exe` and `ZeroTierQuickToggle-portable.exe`.

See `zerotier-ui-linux/README.md` for Linux installation.
