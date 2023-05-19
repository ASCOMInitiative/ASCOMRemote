;
; Script to install the ASCOM Remote Server
;

; Install file names
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

; Specifiy debug or release build;#define public BuildType "Debug" ; Type of build - Release or Debug
#define public BuildType "Release" ; Type of build - Release or Debug

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
Compression = none
DefaultDirName={autopf}\{#MyInstallFolder}
DefaultGroupName={#MyAppName}
MinVersion=6.1SP1
DisableProgramGroupPage=yes
DisableDirPage = no
OutputBaseFilename=ASCOMRemote({#MyAppVersion})Setup
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
Source: "..\publish\x64\*.exe"; DestDir: "{app}"; Flags: ignoreversion signonce; Components: nosupport; Check: Is64BitInstallMode
Source: "..\publish\x64\*.dll"; DestDir: "{app}"; Flags: ignoreversion signonce; Components: nosupport; Check: Is64BitInstallMode
Source: "..\publish\x64\*"; DestDir: "{app}"; Flags: ignoreversion; Excludes:"*.exe,*.dll"; Components: nosupport; Check: Is64BitInstallMode

; 64bit OS - Install the 32bit app
Source: "..\publish\x86\*.exe"; DestDir: "{app}\32bit"; Flags: ignoreversion signonce; Components: nosupport; Check: Is64BitInstallMode
Source: "..\publish\x86\*.dll"; DestDir: "{app}\32bit"; Flags: ignoreversion signonce; Components: nosupport; Check: Is64BitInstallMode
Source: "..\publish\x86\*"; DestDir: "{app}\32bit"; Flags: ignoreversion; Excludes:"*.exe,*.dll";Components: nosupport; Check: Is64BitInstallMode

; 32bit OS - Install the 32bit app
Source: "..\publish\x86\*.exe"; DestDir: "{app}"; Flags: ignoreversion signonce; Components: nosupport; Check: not Is64BitInstallMode
Source: "..\publish\x86\*.dll"; DestDir: "{app}"; Flags: ignoreversion signonce; Components: nosupport; Check: not Is64BitInstallMode
Source: "..\publish\x86\*"; DestDir: "{app}"; Flags: ignoreversion; Excludes:"*.exe,*.dll"; Components: nosupport; Check: not Is64BitInstallMode

; DOCUMENTATION
Source: "..\Documentation\{#ASCOMRemoteDocumentationFileName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
; 64bit OS
Name: "{autoprograms}\ASCOM Remote Server"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\ASCOM.ico"; Check: Is64BitInstallMode
; Name: "{autodesktop}\Remote Server"; Filename: "{app}\64bit\ASCOM.RemoteServer"; Tasks: desktopicon; IconFilename: "{app}\64bit\ASCOM.ico"; Check: Is64BitInstallMode

;32bit OS
Name: "{autoprograms}\ASCOM Remote Server"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\ASCOM.ico"; Check: not Is64BitInstallMode
;Name: "{autodesktop}\Remote Server"; Filename: "{app}\ASCOM.RemoteServer"; Tasks: desktopicon; IconFilename: "{app}\ASCOM.ico"; Check: not Is64BitInstallMode

[Run]

[UninstallRun]

[Registry]

[Components]
Name: "nosupport"; Description: "Stand alone executables"; Types: standalonetype; Flags:  disablenouninstallwarning
Name: "support"; Description: "Executables that require support files"; Types: requiressupporttype; Flags:  disablenouninstallwarning

[Types]
Name: "requiressupporttype"; Description: "Remote Server - Requires .NET 7 support files (7Mb)"
Name: "standalonetype"; Description: "Remote Server - Stand alone installation (140MB)"

[Code]
const
   REQUIRED_PLATFORM_VERSION = 6.5; // Set this to the minimum required ASCOM Platform version for this application
   REQUIRED_DOTNET_VERSION = 'v4.8';   // Set this to the minimum required Microsoft .NET Framework version for this application

var
  LightMsgPage: TOutputMsgWizardPage;
  
//procedure InitializeWizard;
//begin
  { Create the pages }

//  LightMsgPage := CreateOutputMsgPage(wpWelcome,
//   'The ASCOM Remote Clients were superseded in Platform 6.5',
//   '',
//   'Please use Dynamic Clients that are created through the Platform Chooser.');
//end;

//begin
//  { Create the pages }

//  LightMsgPage := CreateOutputMsgPage(wpWelcome,
//   'ASCOM Remote Clients - Please Note', 'The Remote Clients were superseded in Platform 6.5',
//   'The Remote Clients in this installer can be uninstalled but can no longer be selected for installation.' #13#13 +
//   'In place of Remote Clients, please use Dynamic Clients that can be created through the Platform Chooser.' #13#13 + 
//   'Any Remote Clients you have created will continue to function but do not support the new interfaces and features introduced in Platform 6.5.');
//end;




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
// Function to determine whether the specified version of the .NET Framework is installed
//
// Thanks to Christoph Nahr (http://kynosarges.org/DotNetVersion.html) for this code
function IsDotNetDetected(version: string; service: cardinal): boolean;
// version -- Specify one of these strings for the required .NET Framework version:
//    'v1.1'          .NET Framework 1.1                     'v2.0'          .NET Framework 2.0  
//    'v3.0'          .NET Framework 3.0                     'v3.5'          .NET Framework 3.5
//    'v4\Client'     .NET Framework 4.0 Client Profile      'v4\Full'       .NET Framework 4.0 Full Installation  
//    'v4.5'          .NET Framework 4.5                     'v4.5.1'        .NET Framework 4.5.1
//    'v4.5.2'        .NET Framework 4.5.2                   'v4.6'          .NET Framework 4.6
//    'v4.6.1'        .NET Framework 4.6.1                   'v4.6.2'        .NET Framework 4.6.2
//    'v4.7'          .NET Framework 4.7                     'v4.7.1'        .NET Framework 4.7.1
//    'v4.7.2'        .NET Framework 4.7.2                   'v4.8           .NET Framework 4.8
//
// service -- Specify any non-negative integer for the required service pack level:
//    0               No service packs required
//    1, 2, etc.      Service pack 1, 2, etc. required (usually zero for more recent versions)
var
    key, versionKey: string;
    install, release, serviceCount, versionRelease: cardinal;
    success: boolean;
begin
    versionKey := version;
    versionRelease := 0;

    // .NET 1.1 and 2.0 embed release number in version key
    if version = 'v1.1' then begin
        versionKey := 'v1.1.4322';
    end else if version = 'v2.0' then begin
        versionKey := 'v2.0.50727';
    end

    // .NET 4.5 and newer install as update to .NET 4.0 Full
    else if Pos('v4.', version) = 1 then begin
        versionKey := 'v4\Full';
        case version of
          'v4.5':   versionRelease := 378389;
          'v4.5.1': versionRelease := 378675; // 378758 on Windows 8 and older
          'v4.5.2': versionRelease := 379893;
          'v4.6':   versionRelease := 393295; // 393297 on Windows 8.1 and older
          'v4.6.1': versionRelease := 394254; // 394271 before Win10 November Update
          'v4.6.2': versionRelease := 394802; // 394806 before Win10 Anniversary Update
          'v4.7':   versionRelease := 460798; // 460805 before Win10 Creators Update
          'v4.7.1': versionRelease := 461308; // 461310 before Win10 Fall Creators Update
          'v4.7.2': versionRelease := 461808; // 461814 before Win10 April 2018 Update
          'v4.8':   versionRelease := 528040;
        end;
    end;

    // installation key group for all .NET versions
    key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\' + versionKey;

    // .NET 3.0 uses value InstallSuccess in subkey Setup
    if Pos('v3.0', version) = 1 then begin
        success := RegQueryDWordValue(HKLM, key + '\Setup', 'InstallSuccess', install);
    end else begin
        success := RegQueryDWordValue(HKLM, key, 'Install', install);
    end;

    // .NET 4.0 and newer use value Servicing instead of SP
    if Pos('v4', version) = 1 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Servicing', serviceCount);
    end else begin
        success := success and RegQueryDWordValue(HKLM, key, 'SP', serviceCount);
    end;

    // .NET 4.5 and newer use additional value Release
    if versionRelease > 0 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Release', release);
        success := success and (release >= versionRelease);
    end;

    result := success and (install = 1) and (serviceCount >= service);
end;

//
//  Before the installer UI appears, verify that the required ASCOM Platform and .NET Framework versions are installed.
//
function InitializeSetup(): Boolean;
var
   PlatformVersionNumber : double;
begin
   Result := FALSE;  // Assume failure so the installer UI will not appear
   PlatformVersionNumber := PlatformVersion(); // Get the installed Platform version as a double
   If PlatformVersionNumber >= REQUIRED_PLATFORM_VERSION then	// Test whether we have the minimum required Platform
      // We do have a suitable Platform installed
      if IsDotNetDetected(REQUIRED_DOTNET_VERSION, 0) then  // Test whether we have the minimum required .NET Framework version
         // We do have a suitable .NET Framework installed so return TRUE to make the installer UI appear
         Result := TRUE 
      else
         // No or insufficient .NET Framework is installed
         MsgBox('ASCOM Remote requires Microsoft .NET Framework ' + REQUIRED_DOTNET_VERSION + ' or higher' #13#13 'Please use Windows Update to install the latest .NET version and then re-run this setup program.', mbCriticalError, MB_OK) 
   else
         // No or insufficient ASCOM Platform is installed
         if PlatformVersionNumber = 0.0 then
         MsgBox('The ASCOM Platform is not installed.' #13#13 'Please install Platform ' + Format('%3.1f', [REQUIRED_PLATFORM_VERSION]) + ' or later from http://www.ascom-standards.org', mbCriticalError, MB_OK)
      else 
         MsgBox('This version of ASCOM Remote requires ASCOM Platform ' + Format('%3.1f', [REQUIRED_PLATFORM_VERSION]) + ' or later, but Platform '+ Format('%3.1f', [PlatformVersionNumber]) + ' is installed.' #13#13 'Please install the latest Platform before continuing; you will find it at https://www.ascom-standards.org', mbCriticalError, MB_OK);
end;

// Code to enable the installer to uninstall previous versions of itself when a new version is installed
procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  UninstallExe: String;
  UninstallRegistry: String;
begin
  if (CurStep = ssInstall) then // Install step has started
	begin
      // Create the correct registry location name, which is based on the AppId
      UninstallRegistry := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#SetupSetting("AppId")}' + '_is1');
      MsgBox(UninstallRegistry, mbCriticalError, MB_OK);
      // Check whether an extry exists
      if RegQueryStringValue(HKLM, UninstallRegistry, 'UninstallString', UninstallExe) then
       MsgBox('If evaluated to TRUE', mbCriticalError, MB_OK);
     
        begin // Entry exists and previous version is installed so run its uninstaller quietly after informing the user
          MsgBox('About to run uninstaller...', mbCriticalError, MB_OK);
          Exec(RemoveQuotes(UninstallExe), ' /SILENT', '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode);
          sleep(100);    //Give enough time for the install screen to be repainted before continuing
        end
   end;
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = wpSelectComponents then
    begin
      //WizardForm.ComponentsList.Checked[1] := False;
      //if not WizardForm.ComponentsList.Checked[1] then WizardForm.ComponentsList.ItemEnabled[1] := False;
    end;
end;

// Function to determine whether .NET 4.7 is installed
function IsDotNet4Point7Installed: Boolean;
begin
  if IsDotNetDetected('v4.7', 0) then  // Test whether the .NET version 4.7 is installed
    Result := TRUE     // .NET 4.7 or later is installed so return TRUE
  else
    Result := FALSE;   // A .NET version earlier than 4.7 is installed so return FALSE
end;
