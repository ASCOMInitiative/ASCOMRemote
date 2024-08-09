;
; Script to install the ASCOM Remote Server
;

; Pre-define ISPP variables
#define FileHandle
#define FileLine
#define MyInformationVersion

; Read the informational SEMVER version string from the file created by the build process
#define FileHandle = FileOpen("..\publish\remote\InstallerVersion.txt"); 
#define FileLine = FileRead(FileHandle)
#pragma message "Installer version number: " + FileLine

; Save the SEMVER version for use in the installer filename
#define MyInformationVersion FileLine

; Close the SEMVER version file
#if FileHandle
  #expr FileClose(FileHandle)
#endif

; Create other ISPP installer variables 
#define MyAppName "ASCOM Remote"
#define MyAppPublisher "ASCOM Initiative (Peter Simpson)"
#define MyAppPublisherURL "https://ascom-standards.org"
#define MyAppSupportURL "URL=https://ascomtalk.groups.io/g/Developer/topics"
#define MyAppUpdatesURL "https://github.com/ASCOMInitiative/ASCOMRemote/releases"
#define MyAppExeName "RemoteServer.exe"
#define MyAppAuthor "Peter Simpson"
#define MyAppCopyright "Copyright © 2023 " + MyAppAuthor
#define MyAppVersion GetVersionNumbersString("..\publish\x64\RemoteServer.exe")  ; Create version number variable
#define ASCOMRemoteDocumentationFileName "ASCOM Remote Installation and Configuration.pdf"
#define MyInstallFolder = "ASCOM\RemoteServer"

[Setup]
AppID={{0ee690ae-7927-4ee7-b851-f5877c077ff5}
AppCopyright={#MyAppCopyright}
AppName={#MyAppName}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppPublisherURL}
AppSupportURL={#MyAppSupportURL}
AppUpdatesURL={#MyAppUpdatesURL}
AppVerName={#MyAppName}
AppVersion={#MyAppVersion}
ArchitecturesInstallIn64BitMode=x64
Compression=lzma2/max
DefaultDirName={autopf}\{#MyInstallFolder}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
DisableDirPage=yes
MinVersion=6.1SP1
OutputBaseFilename=ASCOMRemote({#MyInformationVersion})Setup
OutputDir=.\Build
PrivilegesRequired=admin
SetupIconFile=ASCOM.ico
SetupLogging=true
SignToolRunMinimized=yes
SignTool = SignASCOMRemote
ShowLanguageDialog=auto
SolidCompression=no
UninstallDisplayName=
UninstallDisplayIcon={app}\{#MyAppExeName}
VersionInfoCompany={#MyAppPublisher}
VersionInfoCopyright={#MyAppAuthor}
VersionInfoDescription= {#MyAppName}
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion= {#MyAppVersion}
VersionInfoVersion={#MyAppVersion}
WizardImageFile=NewWizardImage.bmp
WizardSmallImageFile=ASCOMLogo.bmp
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "armenian"; MessagesFile: "compiler:Languages\Armenian.isl"
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "bulgarian"; MessagesFile: "compiler:Languages\Bulgarian.isl"
Name: "catalan"; MessagesFile: "compiler:Languages\Catalan.isl"
Name: "corsican"; MessagesFile: "compiler:Languages\Corsican.isl"
Name: "czech"; MessagesFile: "compiler:Languages\Czech.isl"
Name: "danish"; MessagesFile: "compiler:Languages\Danish.isl"
Name: "dutch"; MessagesFile: "compiler:Languages\Dutch.isl"
Name: "finnish"; MessagesFile: "compiler:Languages\Finnish.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "hebrew"; MessagesFile: "compiler:Languages\Hebrew.isl"
Name: "icelandic"; MessagesFile: "compiler:Languages\Icelandic.isl"
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"
Name: "norwegian"; MessagesFile: "compiler:Languages\Norwegian.isl"
Name: "polish"; MessagesFile: "compiler:Languages\Polish.isl"
Name: "portuguese"; MessagesFile: "compiler:Languages\Portuguese.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "slovak"; MessagesFile: "compiler:Languages\Slovak.isl"
Name: "slovenian"; MessagesFile: "compiler:Languages\Slovenian.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"
Name: "ukrainian"; MessagesFile: "compiler:Languages\Ukrainian.isl"

[Files]
; 64bit OS - Install the 64bit app
Source: "..\publish\remote\x64\*.exe"; DestDir: "{app}"; Flags: ignoreversion signonce; Check: Is64BitInstallMode 
Source: "..\publish\remote\x64\*.dll"; DestDir: "{app}"; Flags: ignoreversion signonce; Check: Is64BitInstallMode
Source: "..\publish\remote\x64\*"; DestDir: "{app}"; Flags: ignoreversion; Excludes:"*.exe,*.dll"; Check: Is64BitInstallMode

; 64bit OS - Install the 32bit app
Source: "..\publish\remote\x86\*.exe"; DestDir: "{app}\32bit"; Flags: ignoreversion signonce; Check: Is64BitInstallMode
Source: "..\publish\remote\x86\*.dll"; DestDir: "{app}\32bit"; Flags: ignoreversion signonce; Check: Is64BitInstallMode
Source: "..\publish\remote\x86\*"; DestDir: "{app}\32bit"; Flags: ignoreversion; Excludes:"*.exe,*.dll"; Check: Is64BitInstallMode

; 32bit OS - Install the 32bit app
Source: "..\publish\remote\x86\*.exe"; DestDir: "{app}"; Flags: ignoreversion signonce; Check: not Is64BitInstallMode
Source: "..\publish\remote\x86\*.dll"; DestDir: "{app}"; Flags: ignoreversion signonce; Check: not Is64BitInstallMode
Source: "..\publish\remote\x86\*"; DestDir: "{app}"; Flags: ignoreversion; Excludes:"*.exe,*.dll"; Check: not Is64BitInstallMode

;Source: "..\publish\permissions\x64\*.exe"; DestDir: "{app}\SetNetworkPermissions"; Flags: ignoreversion signonce; Check: Is64BitInstallMode 
;Source: "..\publish\permissions\x64\*.dll"; DestDir: "{app}\SetNetworkPermissions"; Flags: ignoreversion signonce; Check: Is64BitInstallMode
;Source: "..\publish\permissions\x64\*"; DestDir: "{app}\SetNetworkPermissions"; Flags: ignoreversion; Excludes:"*.exe,*.dll"; Check: Is64BitInstallMode

; Install the 32bit exe into the 32bit program files folder structure
Source: "..\publish\permissions\x86\*.exe"; DestDir: "{autopf32}\{#MyInstallFolder}\SetNetworkPermissions"; Flags: ignoreversion signonce;
;Source: "..\publish\permissions\x86\*.dll"; DestDir: "{autopf32}\{#MyInstallFolder}\SetNetworkPermissions"; Flags: ignoreversion signonce;
Source: "..\publish\permissions\x86\*"; DestDir: "{autopf32}\{#MyInstallFolder}\SetNetworkPermissions"; Flags: ignoreversion; Excludes:"*.exe,*.dll";

; DOCUMENTATION
Source: "..\Documentation\{#ASCOMRemoteDocumentationFileName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\ASCOM Remote Server"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\ASCOM.ico"
Name: "{autodesktop}\Remote Server"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; IconFilename: "{app}\ASCOM.ico"
[Tasks]
Name: desktopicon; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent unchecked

[UninstallDelete]
Name: "{app}\32bit"; Type: dirifempty
Name: "{app}"; Type: dirifempty

[Code]
const
 REQUIRED_PLATFORM_VERSION = 6.5; // Set this to the minimum required ASCOM Platform version for this application

//
// Function to return the ASCOM Platform's version number as a double.
//
function PlatformVersion(): Extended;
var
   PlatVerString : String;
begin
   Result := 0.0;  // Initialise the return value in case we can't read the registry
   try
      if RegQueryStringValue(HKEY_LOCAL_MACHINE_32, 'Software\ASCOM','PlatformVersion', PlatVerString) then 
      begin // Successfully read the value from the registry
         Result := StrToFloat(PlatVerString); // Create a double from the X.Y Platform version string
      end;
   except                                                                   
      ShowExceptionMessage;
      Result:= -1.0; // Indicate in the return value that an exception was generated
   end;
end;

//
//  Before the installer UI appears, verify that the required ASCOM Platform and .NET Framework versions are installed.
//
function InitializeSetup(): Boolean;
var
   PlatformVersionNumber : double;

begin
  Result := FALSE;  // Assume failure so the installer UI will not appear
  
  // Get the installed Platform version as a double
  PlatformVersionNumber := PlatformVersion();
  
  // Test whether we have the minimum required Platform
  If PlatformVersionNumber >= REQUIRED_PLATFORM_VERSION then begin

    // We do have a suitable Platform installed
    Result := TRUE;
  end
  else begin // No or insufficient ASCOM Platform is installed

    // Test whether a Platform is installed at all
    if PlatformVersionNumber = 0.0 then begin // No platform is installed
      MsgBox('The ASCOM Platform is not installed.' #13#13 'Please install Platform ' + Format('%3.1f', [REQUIRED_PLATFORM_VERSION]) + ' or later from http://www.ascom-standards.org', mbCriticalError, MB_OK)
    end
    else begin // A Platform below the minimim requirement is installed
      MsgBox('This version of ASCOM Remote requires ASCOM Platform ' + Format('%3.1f', [REQUIRED_PLATFORM_VERSION]) + ' or later, but Platform '+ Format('%3.1f', [PlatformVersionNumber]) + ' is installed.' #13#13 'Please install the latest Platform before continuing; you will find it at https://www.ascom-standards.org', mbCriticalError, MB_OK);
    end;
  end;
end;

// Code to enable the installer to uninstall previous versions of itself when a new version is installed
procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  UninstallExe: String;
  UninstallRegistry: String;

begin
  if (CurStep = ssInstall) then	begin

    // Create the correct registry location name, which is based on the AppId
    UninstallRegistry := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#SetupSetting("AppId")}' + '_is1');

    // Check whether the previous install was the original 32bit only version
    if Is64BitInstallMode then begin

      // Check whether an extry exists in the 32bit registry
      if RegQueryStringValue(HKLM32, UninstallRegistry, 'UninstallString', UninstallExe) then begin

        // 32bit setup entry exists so run its uninstaller quietly after informing the user
        Exec(RemoveQuotes(UninstallExe), ' /SILENT', '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode);
      end;
    end;
    
    // Check whether the previous install was a new version either (both 32bit  and 64bit installs)
    if RegQueryStringValue(HKLM, UninstallRegistry, 'UninstallString', UninstallExe) then begin
    
        // 64bit entry exists so run its uninstaller quietly after informing the user
        Exec(RemoveQuotes(UninstallExe), ' /SILENT', '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode);
      end;
    end;
end;