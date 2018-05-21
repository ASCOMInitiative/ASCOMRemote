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
DefaultGroupName=ASCOM REST Server
DisableDirPage=yes
DisableProgramGroupPage=yes
; Must be at least Vista to run
MinVersion=6.0  
OutputDir="Build"
#emit "OutputBaseFilename=ASCOMRemote(" + MyAppVer +")setup"
PrivilegesRequired=admin
SetupIconFile=ASCOM.ico
SetupLogging=true
SolidCompression=yes
UninstallDisplayIcon={app}\WebClient\ASCOM.WebClient.LocalServer.exe.exe
UninstallFilesDir="{cf}\ASCOM\Uninstall\Remote"
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
; LOCAL SERVER AND DRIVER SUPPORT FILES
Source: "..\Remote Client Device Base Classes\bin\Release\ASCOM.RemoteClientBaseClasses.dll"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Device Base Classes\bin\Release\ASCOM.RemoteClientBaseClasses.pdb"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Local Server\bin\Release\RestSharp.dll"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Local Server\bin\Release\Newtonsoft.Json.dll"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
; CAMERA DRIVERS
Source: "..\Remote Client Camera Device 1\bin\Release\ASCOM.Remote1.Camera.dll"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Camera Device 1\bin\Release\ASCOM.Remote1.Camera.pdb"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Camera Device 2\bin\Release\ASCOM.Remote2.Camera.dll"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Camera Device 2\bin\Release\ASCOM.Remote2.Camera.pdb"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
; DOME DRIVERS
Source: "..\Remote Client Dome Device 1\bin\Release\ASCOM.Remote1.Dome.dll"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Dome Device 1\bin\Release\ASCOM.Remote1.Dome.pdb"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Dome Device 2\bin\Release\ASCOM.Remote2.Dome.dll"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Dome Device 2\bin\Release\ASCOM.Remote2.Dome.pdb"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
; FILTER WHEEL DRIVERS
Source: "..\Remote Client FilterWheel Device 1\bin\Release\ASCOM.Remote1.FilterWheel.dll"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client FilterWheel Device 1\bin\Release\ASCOM.Remote1.FilterWheel.pdb"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client FilterWheel Device 2\bin\Release\ASCOM.Remote2.FilterWheel.dll"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client FilterWheel Device 2\bin\Release\ASCOM.Remote2.FilterWheel.pdb"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
; FOCUSER DRIVERS
Source: "..\Remote Client Focuser Device 1\bin\Release\ASCOM.Remote1.Focuser.dll"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Focuser Device 1\bin\Release\ASCOM.Remote1.Focuser.pdb"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Focuser Device 2\bin\Release\ASCOM.Remote2.Focuser.dll"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Focuser Device 2\bin\Release\ASCOM.Remote2.Focuser.pdb"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
; OBSERVINGCONDITIONS DRIVERS
Source: "..\Remote Client ObservingConditions Device 1\bin\Release\ASCOM.Remote1.ObservingConditions.dll"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client ObservingConditions Device 1\bin\Release\ASCOM.Remote1.ObservingConditions.pdb"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client ObservingConditions Device 2\bin\Release\ASCOM.Remote2.ObservingConditions.dll"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client ObservingConditions Device 2\bin\Release\ASCOM.Remote2.ObservingConditions.pdb"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
; ROTATOR DRIVERS
Source: "..\Remote Client Rotator Device 1\bin\Release\ASCOM.Remote1.Rotator.dll"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Rotator Device 1\bin\Release\ASCOM.Remote1.Rotator.pdb"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Rotator Device 2\bin\Release\ASCOM.Remote2.Rotator.dll"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Rotator Device 2\bin\Release\ASCOM.Remote2.Rotator.pdb"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
; SAFETYMONITOR DRIVERS
Source: "..\Remote Client SafetyMonitor Device 1\bin\Release\ASCOM.Remote1.SafetyMonitor.dll"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client SafetyMonitor Device 1\bin\Release\ASCOM.Remote1.SafetyMonitor.pdb"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client SafetyMonitor Device 2\bin\Release\ASCOM.Remote2.SafetyMonitor.dll"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client SafetyMonitor Device 2\bin\Release\ASCOM.Remote2.SafetyMonitor.pdb"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
; SWITCH DRIVERS
Source: "..\Remote Client Switch Device 1\bin\Release\ASCOM.Remote1.Switch.dll"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Switch Device 1\bin\Release\ASCOM.Remote1.Switch.pdb"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Switch Device 2\bin\Release\ASCOM.Remote2.Switch.dll"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Switch Device 2\bin\Release\ASCOM.Remote2.Switch.pdb"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
; TELESCOPE DRIVERS
Source: "..\Remote Client Telescope Device 1\bin\Release\ASCOM.Remote1.Telescope.dll"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Telescope Device 1\bin\Release\ASCOM.Remote1.Telescope.pdb"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Telescope Device 2\bin\Release\ASCOM.Remote2.Telescope.dll"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Remote Client Telescope Device 2\bin\Release\ASCOM.Remote2.Telescope.pdb"; DestDir: "{app}\RemoteClients"; Flags: ignoreversion; Components: ClientComponents
; REST SERVER FILES
Source: "..\REST Server\bin\Release\ASCOM.RESTServer.exe"; DestDir: "{pf}\ASCOM\RESTServer"; Flags: ignoreversion; Components: ServerComponents
Source: "..\REST Server\bin\Release\ASCOM.RESTServer.pdb"; DestDir: "{pf}\ASCOM\RESTServer"; Flags: ignoreversion; Components: ServerComponents
; REST SERVER SUPPORT FILES
Source: "..\REST Server\bin\Release\RestSharp.dll"; DestDir: "{pf}\ASCOM\RESTServer"; Flags: ignoreversion; Components: ServerComponents
Source: "..\REST Server\bin\Release\Newtonsoft.Json.dll"; DestDir: "{pf}\ASCOM\RESTServer"; Flags: ignoreversion; Components: ServerComponents
; SET NETWORK PERMISSIONS FILES
Source: "..\SetNetworkPermissions\bin\Release\ASCOM.SetNetworkPermissions.exe"; DestDir: "{pf}\ASCOM\Remote"; Flags: ignoreversion; Components: ServerComponents
Source: "..\SetNetworkPermissions\bin\Release\ASCOM.SetNetworkPermissions.pdb"; DestDir: "{pf}\ASCOM\Remote"; Flags: ignoreversion; Components: ServerComponents
; SET NETWORK PERMISSIONS SUPPORT FILES
Source: "..\SetNetworkPermissions\bin\Release\WindowsFirewallHelper.dll"; DestDir: "{pf}\ASCOM\Remote"; Flags: ignoreversion; Components: ServerComponents
Source: "..\SetNetworkPermissions\bin\Release\CommandLine.dll"; DestDir: "{pf}\ASCOM\Remote"; Flags: ignoreversion; Components: ServerComponents
Source: "..\SetNetworkPermissions\bin\Release\Interop.NetFwTypeLib.dll"; DestDir: "{pf}\ASCOM\Remote"; Flags: ignoreversion; Components: ServerComponents
; WEB CONTENT
;Source: "..\Web Content\index.html"; DestDir: "{pf}\ASCOM\RemoteServer"; Components: WebContent

[Run]
Filename: "{app}\RemoteClients\ASCOM.RemoteClientLocalServer.exe"; Parameters: "/regserver"
Filename: "{pf}\ASCOM\Remote\ASCOM.SetNetworkPermissions.exe"; Parameters: "--localserverpath ""{app}\RemoteClients\ASCOM.RemoteClientLocalServer.exe"""; Components: ClientComponents; Flags: runhidden
Filename: "{pf}\ASCOM\Remote\ASCOM.SetNetworkPermissions.exe"; Parameters: "--remoteserverpath ""{pf}\ASCOM\RESTServer\ASCOM.RESTServer.exe"""; Components: ServerComponents; Flags: runhidden

[UninstallRun]
Filename: "{app}\RemoteClients\ASCOM.RemoteClientLocalServer.exe"; Parameters: "/unregserver"

[Registry]

[Icons]
Name: "{group}\ASCOM REST Server"; Filename: "{pf}\ASCOM\RESTServer\ASCOM.RESTServer.exe"

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
// Before the installer UI appears, verify that the required ASCOM Platform version is installed.
//
function InitializeSetup(): Boolean;
var
   PlatformVersionNumber : double;
 begin
   Result := FALSE;  // Assume failure
   PlatformVersionNumber := PlatformVersion(); // Get the installed Platform version as a double
   If PlatformVersionNumber >= REQUIRED_PLATFORM_VERSION then	// Check whether we have the minimum required Platform or newer
      Result := TRUE
   else
      if PlatformVersionNumber = 0.0 then
         MsgBox('No ASCOM Platform is installed. Please install Platform ' + Format('%3.1f', [REQUIRED_PLATFORM_VERSION]) + ' or later from http://www.ascom-standards.org', mbCriticalError, MB_OK)
      else 
         MsgBox('ASCOM Platform ' + Format('%3.1f', [REQUIRED_PLATFORM_VERSION]) + ' or later is required, but Platform '+ Format('%3.1f', [PlatformVersionNumber]) + ' is installed. Please install the latest Platform before continuing; you will find it at http://www.ascom-standards.org', mbCriticalError, MB_OK);
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