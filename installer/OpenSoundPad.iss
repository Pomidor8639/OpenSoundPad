[Setup]
AppId={{5D9E5E9E-758B-4C9D-99C4-325A62E8A51C}
AppName=OpenSoundPad
AppVersion=1.7.0
AppVerName=OpenSoundPad v1.7.0
AppPublisher=OSP OpenSoundPad Community
AppPublisherURL=https://github.com/Pomidor8639/OpenSoundPad
AppSupportURL=https://github.com/Pomidor8639/OpenSoundPad
AppUpdatesURL=https://github.com/Pomidor8639/OpenSoundPad/releases
DefaultDirName={autopf}\OpenSoundPad
DefaultGroupName=OpenSoundPad
AllowNoIcons=yes
OutputDir=Output
OutputBaseFilename=OpenSoundPad-Setup-v1.7.0
SetupIconFile=..\app_icon.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\app_icon.ico

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkedonce
Name: "installvbcable"; Description: "Установить виртуальный аудиокабель VB-CABLE (для Discord, CS2, Telegram)"; GroupDescription: "Драйверы:"; Flags: checkedonce

[Files]
Source: "..\publish\osp-v10\OpenSoundPad.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\app_icon.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "vbcable\*"; DestDir: "{app}\vbcable"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autodesktop}\OpenSoundPad"; Filename: "{app}\OpenSoundPad.exe"; IconFilename: "{app}\app_icon.ico"; Tasks: desktopicon
Name: "{autoprograms}\OpenSoundPad\OpenSoundPad"; Filename: "{app}\OpenSoundPad.exe"; IconFilename: "{app}\app_icon.ico"
Name: "{autoprograms}\OpenSoundPad\VB-CABLE Driver Setup"; Filename: "{app}\vbcable\VBCABLE_Setup_x64.exe"
Name: "{autoprograms}\OpenSoundPad\VB-CABLE Control Panel"; Filename: "{app}\vbcable\VBCABLE_ControlPanel.exe"
Name: "{autoprograms}\OpenSoundPad\Удалить OpenSoundPad"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\vbcable\VBCABLE_Setup_x64.exe"; Description: "Установить драйвер виртуального кабеля VB-CABLE"; Tasks: installvbcable; Flags: waituntilterminated runascurrentuser
Filename: "{app}\OpenSoundPad.exe"; Description: "{cm:LaunchProgram,OpenSoundPad}"; Flags: nowait postinstall skipifsilent
