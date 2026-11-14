#define AppName "ZeroTier Quick Toggle"
#define AppVersion "1.0.0"
#define AppPublisher "kernelsy-labs"
#define AppExeName "ZeroTierQuickToggle.exe"

[Setup]
AppId={{8B5D73EE-5A86-4AB4-9D8A-7D7A1C0E7A10}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\ZeroTier Quick Toggle
DefaultGroupName={#AppName}
OutputDir=..\artifacts
OutputBaseFilename=ZeroTierQuickToggle-Setup
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin
UninstallDisplayIcon={app}\{#AppExeName}
WizardStyle=modern

[Files]
Source: "..\ZeroTierQuickToggle\publish\{#AppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Launch {#AppName}"; Flags: nowait postinstall skipifsilent
