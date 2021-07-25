;
; Script to install the ASCOM Remote Drivers and ASCOM Remote Server
;

; Install direrctory names
#define public RemoteClientDirectory "{app}\RemoteClients" ; Target directory where remote clients will be installed
#define public RemoteServerDirectory "{commonpf}\ASCOM\Remote" ; Target directory where the remote server will be installed

; Install file names
#define public RemoteClientLocalServerName "ASCOM.RemoteClientLocalServer" ; Remote client local server name
#define public RemoteServerName "ASCOM.RemoteServer" ; Remote server application name
#define public ASCOMStandardName "ASCOMStandard" ; ASCOMStandard support DLL name
#define public SetNetworkPermissionsName "ASCOM.SetNetworkPermissions" ; Firewall configuration application name
#define public DynamicRemoteClientsName "ASCOM.DynamicRemoteClients" ; Remote client management application name
#define public RemoteClientBaseClassesName "ASCOM.RemoteClientBaseClasses" ; Remote client support DLL name
#define public ASCOMRemoteDocumentationFileName "ASCOM Remote Installation and Configuration.pdf"; ASCOM Remote documentation file

; Specifiy debug or release build;#define public BuildType "Debug" ; Type of build - Release or Debug
#define public BuildType "Release" ; Type of build - Release or Debug

[Setup]
AppID={{0ee690ae-7927-4ee7-b851-f5877c077ff5}
#define MyAppVer GetFileVersion("..\Remote Server\bin\Release\ASCOM.RemoteServer.exe") ; define variable

AppName=ASCOM Remote
AppCopyright=Copyright © 2021 ASCOM Initiative
AppPublisher=ASCOM Initiative
AppPublisherURL=mailto:peter@peterandjill.co.uk
AppSupportURL=https://ascomtalk.groups.io/g/Help/topics
AppUpdatesURL=https://github.com/ASCOMInitiative/ASCOMRemote/releases
#emit "AppVerName=ASCOM Remote " + MyAppVer + " ("+ BuildType + ")"
#emit "AppVersion=" + MyAppVer
Compression=lzma
DefaultDirName="{commoncf}\ASCOM"
DefaultGroupName="ASCOM Remote"
DisableDirPage=yes
DisableProgramGroupPage=no
; Must be at least Windows 7 SP1 or later to run
MinVersion=6.1.7601 
OutputDir="Build"
#emit "OutputBaseFilename=ASCOMRemote(" + MyAppVer +")setup"
PrivilegesRequired=admin
SetupIconFile=ASCOM.ico
SetupLogging=true
SolidCompression=yes
UninstallDisplayIcon={commonpf}\ASCOM\Remote\ASCOM.ico
UninstallFilesDir="{commoncf}\ASCOM\Uninstall\Remote"
UsePreviousAppDir=no
UsePreviousGroup=no
VersionInfoCompany=Peter Simpson
VersionInfoCopyright=Peter Simpson
VersionInfoDescription=ASCOM Remote
VersionInfoProductName=ASCOM Remote
WizardImageFile=NewWizardImage.bmp
WizardSmallImageFile=ASCOMLogo.bmp
#emit "VersionInfoProductVersion=" + MyAppVer
#emit "VersionInfoVersion=" + MyAppVer
SignTool = SignASCOMRemote

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Dirs]
Name: "{commoncf}\ASCOM\Uninstall\Remote"

[Files]
; LOCAL SERVER FILES
;Source: "..\Remote Client Local Server\bin\{#BuildType}\{#RemoteClientLocalServerName}.exe"; DestDir: "{#RemoteClientDirectory}"; Flags: ignoreversion; Components: ClientComponents
;Source: "..\Remote Client Local Server\bin\{#BuildType}\{#RemoteClientLocalServerName}.pdb"; DestDir: "{#RemoteClientDirectory}"; Flags: ignoreversion; Components: ClientComponents

; DYNAMIC CLIENT GENERATOR
;Source: "..\ASCOM.DynamicRemoteClients\bin\{#BuildType}\{#DynamicRemoteClientsName}.exe"; DestDir: "{#RemoteClientDirectory}"; Flags: ignoreversion; Components: ClientComponents
;Source: "..\ASCOM.DynamicRemoteClients\bin\{#BuildType}\{#DynamicRemoteClientsName}.pdb"; DestDir: "{#RemoteClientDirectory}"; Flags: ignoreversion; Components: ClientComponents

; LOCAL SERVER AND DRIVER SUPPORT FILES
;Source: "..\Remote Client Device Base Classes\bin\{#BuildType}\{#RemoteClientBaseClassesName}.dll"; DestDir: "{#RemoteClientDirectory}"; Flags: ignoreversion; Components: ClientComponents
;Source: "..\Remote Client Device Base Classes\bin\{#BuildType}\{#RemoteClientBaseClassesName}.pdb"; DestDir: "{#RemoteClientDirectory}"; Flags: ignoreversion; Components: ClientComponents
;Source: "..\Remote Client Local Server\bin\{#BuildType}\RestSharp.dll"; DestDir: "{#RemoteClientDirectory}"; Flags: ignoreversion; Components: ClientComponents
;Source: "..\Remote Client Local Server\bin\{#BuildType}\Newtonsoft.Json.dll"; DestDir: "{#RemoteClientDirectory}"; Flags: ignoreversion; Components: ClientComponents

; REMOTE SERVER FILES
Source: "..\Remote Server\bin\{#BuildType}\{#RemoteServerName}.exe"; DestDir: "{#RemoteServerDirectory}"; Flags: ignoreversion; Components: ServerComponents
Source: "..\Remote Server\bin\{#BuildType}\{#RemoteServerName}.exe.config"; DestDir: "{#RemoteServerDirectory}"; Flags: ignoreversion; Components: ServerComponents
Source: "..\Remote Server\bin\{#BuildType}\{#RemoteServerName}.pdb"; DestDir: "{#RemoteServerDirectory}"; Flags: ignoreversion; Components: ServerComponents
Source: "..\Remote Server\ASCOMAlpacaMidRes.jpg"; DestDir: "{#RemoteServerDirectory}"; Flags: ignoreversion; Components: ServerComponents
Source: "..\Remote Server\ascomicon.ico"; DestDir: "{#RemoteServerDirectory}"; Flags: ignoreversion; Components: ServerComponents
; Source: "..\Remote Server\bin\{#BuildType}\{#ASCOMStandardName}.dll"; DestDir: "{#RemoteServerDirectory}"; Flags: ignoreversion; Components: ServerComponents
; Source: "..\Remote Server\bin\{#BuildType}\{#ASCOMStandardName}.pdb"; DestDir: "{#RemoteServerDirectory}"; Flags: ignoreversion; Components: ServerComponents

; REMOTE SERVER SUPPORT FILES
Source: "..\Remote Server\bin\{#BuildType}\Newtonsoft.Json.dll"; DestDir: "{#RemoteServerDirectory}"; Flags: ignoreversion; Components: ServerComponents

; SET NETWORK PERMISSIONS FILES
Source: "..\SetNetworkPermissions\bin\{#BuildType}\{#SetNetworkPermissionsName}.exe"; DestDir: "{#RemoteServerDirectory}"; Flags: ignoreversion; Components: ServerComponents 
Source: "..\SetNetworkPermissions\bin\{#BuildType}\{#SetNetworkPermissionsName}.pdb"; DestDir: "{#RemoteServerDirectory}"; Flags: ignoreversion; Components: ServerComponents

; SET NETWORK PERMISSIONS SUPPORT FILES
Source: "..\SetNetworkPermissions\bin\{#BuildType}\WindowsFirewallHelper.dll"; DestDir: "{#RemoteServerDirectory}"; Flags: ignoreversion; Components: ServerComponents
Source: "..\SetNetworkPermissions\bin\{#BuildType}\CommandLine.dll"; DestDir: "{#RemoteServerDirectory}"; Flags: ignoreversion; Components: ServerComponents
; The following file is required on installations where only .NET 4.6 is installed. This is because the netstandard.dll is only loaded into the GAC in .NET version 4.7 and later
Source: "..\SetNetworkPermissions\bin\{#BuildType}\netstandard.dll"; DestDir: "{#RemoteServerDirectory}"; Flags: ignoreversion; Components: ServerComponents; Check: not IsDotNet4Point7Installed

; DOCUMENTATION
Source: "..\Documentation\{#ASCOMRemoteDocumentationFileName}"; DestDir: "{#RemoteServerDirectory}"; Flags: ignoreversion

; INSTALLER SUPPORT FILES
Source: "ASCOM.ico"; DestDir: "{#RemoteServerDirectory}"; Flags: ignoreversion

; WEB CONTENT
;Source: "..\Web Content\index.html"; DestDir: "{commonpf}\ASCOM\RemoteServer"; Components: WebContent

[Run]
;Filename: "{#RemoteClientDirectory}\{#RemoteClientLocalServerName}.exe"; Parameters: "/regserver"; Components: ClientComponents
;Filename: "{#RemoteClientDirectory}\{#DynamicRemoteClientsName}.exe"; Parameters: "/installersetup"; Components: ClientComponents
;Filename: "{#RemoteServerDirectory}\{#SetNetworkPermissionsName}.exe"; Parameters: "--localserverpath ""{#RemoteClientDirectory}\{#RemoteClientLocalServerName}.exe"""; Components: ClientComponents; Flags: runhidden

[UninstallRun]
;Filename: "{#RemoteClientDirectory}\{#RemoteClientLocalServerName}.exe"; Parameters: "/unregserver"; Components: ClientComponents

[Registry]

[Icons]
Name: "{group}\ASCOM Remote Documentation"; Filename: "{#RemoteServerDirectory}\{#ASCOMRemoteDocumentationFileName}";
Name: "{group}\Remote Server"; Filename: "{#RemoteServerDirectory}\{#RemoteServerName}.exe"; Components: ServerComponents
;Name: "{group}\Remote Client Configuration"; Filename: "{#RemoteClientDirectory}\{#DynamicRemoteClientsName}.exe"; Components: ClientComponents

[Components]
Name: "ServerComponents"; Description: "Remote Server"; Flags: disablenouninstallwarning
;Name: "ClientComponents"; Description: "Remote Clients"; Flags: disablenouninstallwarning

[Types]
Name: "Custom"; Description: "Custom"; Flags: iscustom

[PreCompile]
Name: "..\BuildRemote.cmd"; Flags: cmdprompt

[Code]
const
   REQUIRED_PLATFORM_VERSION = 6.5;    // Set this to the minimum required ASCOM Platform version for this application
   REQUIRED_DOTNET_VERSION = 'v4.8';  // Set this to the minimum required Microsoft .NET Framework version for this application

var
  LightMsgPage: TOutputMsgWizardPage;
  
procedure InitializeWizard;
begin
  { Create the pages }

  LightMsgPage := CreateOutputMsgPage(wpWelcome,
    'ASCOM Remote Clients - Please Note', 'The Remote clients have been superseded in Platform 6.5',
    'The Remote Clients are no longer included in this installer, please use the Dynamic Clients in Platform 6.5 instead.' #13#13 +
    'Dynamic clients can be created through the Platform 6.5 Chooser.');
end;

//
// Function to return the ASCOM Platform's version number as a double.
//
function PlatformVersion(): Double;
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
         MsgBox('ASCOM Platform ' + Format('%3.1f', [REQUIRED_PLATFORM_VERSION]) + ' or later is required, but Platform '+ Format('%3.1f', [PlatformVersionNumber]) + ' is installed.' #13#13 'Please install the latest Platform before continuing; you will find it at http://www.ascom-standards.org', mbCriticalError, MB_OK);
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
      // Check whether an extry exists
      if RegQueryStringValue(HKLM, UninstallRegistry, 'UninstallString', UninstallExe) then
        begin // Entry exists and previous version is installed so run its uninstaller quietly after informing the user
          Exec(RemoveQuotes(UninstallExe), ' /SILENT', '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode);
          sleep(100);    //Give enough time for the install screen to be repainted before continuing
        end
   end;
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = wpSelectComponents then
    begin
      //WizardForm.ComponentsList.Checked[2] := False;
      //WizardForm.ComponentsList.ItemEnabled[2] := False;
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
