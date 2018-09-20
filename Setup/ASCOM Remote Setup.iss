;
; Script to sinatll the ASCOM Remote Drivers and REST Server
;

[Setup]
AppID={{0ee690ae-7927-4ee7-b851-f5877c077ff5}
#define MyAppVer GetFileVersion("..\REST Server\bin\Release\ASCOM.RESTServer.exe") ; define variable

AppName=ASCOM Remote Driver Server 
AppCopyright=Copyright © 2018 ASCOM Initiative
AppPublisher=ASCOM Initiative
AppPublisherURL=mailto:peter@peterandjill.co.uk
AppSupportURL=http://tech.groups.yahoo.com/group/ASCOM-Talk/
AppUpdatesURL=http://ascom-standards.org/
#emit "AppVerName=ASCOM Remote Driver Server " + MyAppVer
#emit "AppVersion=" + MyAppVer
Compression=lzma
DefaultDirName="{cf}\ASCOM"
DefaultGroupName="ASCOM Remote"
DisableDirPage=yes
DisableProgramGroupPage=yes
; Must be at least Windows 7 SP1 or later to run
MinVersion=6.1.7601 
OutputDir="Build"
#emit "OutputBaseFilename=ASCOMRemote(" + MyAppVer +")setup"
PrivilegesRequired=admin
SetupIconFile=ASCOM.ico
SetupLogging=true
SolidCompression=yes
UninstallDisplayIcon={pf}\ASCOM\Remote\ASCOM.ico
UninstallFilesDir="{cf}\ASCOM\Uninstall\Remote"
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

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Dirs]
Name: "{cf}\ASCOM\Uninstall\Remote"

[Files]
; LOCAL SERVER FILES
Source: "..\Remote Client Local Server\bin\Release\ASCOM.RemoteClientLocalServer.exe"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Local Server\bin\Release\ASCOM.RemoteClientLocalServer.pdb"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
; DYNAMIC CLIENT GENERATOR
Source: "..\ASCOM.DynamicRemoteClients\bin\Release\ASCOM.DynamicRemoteClients.exe"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\ASCOM.DynamicRemoteClients\bin\Release\ASCOM.DynamicRemoteClients.pdb"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
; LOCAL SERVER AND DRIVER SUPPORT FILES
Source: "..\Remote Client Device Base Classes\bin\Release\ASCOM.RemoteClientBaseClasses.dll"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Device Base Classes\bin\Release\ASCOM.RemoteClientBaseClasses.pdb"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Local Server\bin\Release\RestSharp.dll"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Local Server\bin\Release\Newtonsoft.Json.dll"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
; REST SERVER FILES
Source: "..\REST Server\bin\Release\ASCOM.RESTServer.exe"; DestDir: "{pf}\ASCOM\Remote"; Flags: ignoreversion; Components: ServerComponents
Source: "..\REST Server\bin\Release\ASCOM.RESTServer.pdb"; DestDir: "{pf}\ASCOM\Remote"; Flags: ignoreversion; Components: ServerComponents
; REST SERVER SUPPORT FILES
Source: "..\REST Server\bin\Release\Newtonsoft.Json.dll"; DestDir: "{pf}\ASCOM\Remote"; Flags: ignoreversion; Components: ServerComponents
; SET NETWORK PERMISSIONS FILES
Source: "..\SetNetworkPermissions\bin\Release\ASCOM.SetNetworkPermissions.exe"; DestDir: "{pf}\ASCOM\Remote"; Flags: ignoreversion; Components: ServerComponents
Source: "..\SetNetworkPermissions\bin\Release\ASCOM.SetNetworkPermissions.pdb"; DestDir: "{pf}\ASCOM\Remote"; Flags: ignoreversion; Components: ServerComponents
; SET NETWORK PERMISSIONS SUPPORT FILES
Source: "..\SetNetworkPermissions\bin\Release\WindowsFirewallHelper.dll"; DestDir: "{pf}\ASCOM\Remote"; Flags: ignoreversion; Components: ServerComponents
Source: "..\SetNetworkPermissions\bin\Release\CommandLine.dll"; DestDir: "{pf}\ASCOM\Remote"; Flags: ignoreversion; Components: ServerComponents
Source: "..\SetNetworkPermissions\bin\Release\Interop.NetFwTypeLib.dll"; DestDir: "{pf}\ASCOM\Remote"; Flags: ignoreversion; Components: ServerComponents
; DOCUMENTATION
Source: "..\Documentation\ASCOM Remote Concept.pdf"; DestDir: "{pf}\ASCOM\Remote"; Flags: ignoreversion
; INSTALLER SUPPORT FILES
Source: "ASCOM.ico"; DestDir: "{pf}\ASCOM\Remote"; Flags: ignoreversion
; WEB CONTENT
;Source: "..\Web Content\index.html"; DestDir: "{pf}\ASCOM\RemoteServer"; Components: WebContent

[Run]
Filename: "{app}\RemoteClients\ASCOM.RemoteClientLocalServer.exe"; Parameters: "/regserver"; Components: ClientComponents
Filename: "{app}\RemoteClients\ASCOM.DynamicRemoteClients.exe"; Parameters: "/installersetup"; Components: ClientComponents
Filename: "{pf}\ASCOM\Remote\ASCOM.SetNetworkPermissions.exe"; Parameters: "--localserverpath ""{app}\RemoteClients\ASCOM.RemoteClientLocalServer.exe"""; Components: ClientComponents; Flags: runhidden
Filename: "{pf}\ASCOM\Remote\ASCOM.SetNetworkPermissions.exe"; Parameters: "--remoteserverpath ""{pf}\ASCOM\RESTServer\ASCOM.RESTServer.exe"""; Components: ServerComponents; Flags: runhidden

[UninstallRun]
Filename: "{app}\RemoteClients\ASCOM.RemoteClientLocalServer.exe"; Parameters: "/unregserver"; Components: ClientComponents

[Registry]

[Icons]
Name: "{group}\ASCOM REST Server"; Filename: "{pf}\ASCOM\Remote\ASCOM Remote Concept.pdf";
Name: "{group}\ASCOM Remote Documentation"; Filename: "{pf}\ASCOM\Remote\ASCOM.RESTServer.exe"; Components: ServerComponents
Name: "{group}\Remote Client Configuration"; Filename: "{app}\RemoteClients\ASCOM.DynamicRemoteClients.exe"; Components: ClientComponents

[Components]
Name: "ClientComponents"; Description: "Client components"; Types: ClientOnly Full;
Name: "ServerComponents"; Description: "Server components"; Types: Full ServerOnly;
;Name: "WebContent"; Description: "Web server content files"; Types: Full ServerOnly;

[Types]
Name: "Full"; Description: "Client and server components"
Name: "ClientOnly"; Description: "Client components only"
Name: "ServerOnly"; Description: "Server components only"
Name: "Custom"; Description: "Custom"; Flags: iscustom

[Code]
const
   REQUIRED_PLATFORM_VERSION = 6.2;    // Set this to the minimum required ASCOM Platform version for this application
   REQUIRED_DOTNET_VERSION = 'v4.6.2';  // Set this to the minimum required Microsoft .NET Framework version for this application

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
//    'v4.7.2'        .NET Framework 4.7.2
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