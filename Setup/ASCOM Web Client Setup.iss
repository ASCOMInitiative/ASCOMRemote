;
; Script to sinatll the ASCOM Remote Access Drivers and Server
;

[Setup]
AppID={{0ee690ae-7927-4ee7-b851-f5877c077ff5}
#define MyAppVer GetFileVersion("..\Remote Device Server\bin\Release\ASCOM.RemoteDeviceServer.exe") ; define variable

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
DefaultGroupName=ASCOM Remote Driver Server
DisableDirPage=yes
DisableProgramGroupPage=yes
; Must be at least Vista to run
MinVersion=6.0  
OutputDir="Build"
#emit "OutputBaseFilename=ASCOMRemoteDriverServer(" + MyAppVer +")setup"
PrivilegesRequired=admin
SetupIconFile=ASCOM.ico
SetupLogging=true
SolidCompression=yes
UninstallDisplayIcon={app}\WebClient\ASCOM.WebClient.LocalServer.exe.exe
UninstallFilesDir="{cf}\ASCOM\Uninstall\Telescope\Web"
VersionInfoCompany=Peter Simpson
VersionInfoCopyright=Peter Simpson
VersionInfoDescription=ASCOM Remote Driver Server
VersionInfoProductName=ASCOM Remote Driver Server
WizardImageFile=NewWizardImage.bmp
WizardSmallImageFile=ASCOMLogo.bmp
#emit "VersionInfoProductVersion=" + MyAppVer
#emit "VersionInfoVersion=" + MyAppVer

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Dirs]
Name: "{cf}\ASCOM\Uninstall\Telescope\Web"

[Files]
; LOCAL SERVER FILES
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient.LocalServer.exe"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient.LocalServer.pdb"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
; LOCAL SERVER AND DRIVER SUPPORT FILES
Source: "..\Web Client Local Server\bin\Release\Web Client Device Base Classes.dll"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\Web Client Device Base Classes.pdb"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\RestSharp.dll"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\Newtonsoft.Json.dll"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
; CAMERA DRIVERS
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient1.Camera.dll"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient1.Camera.pdb"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient2.Camera.dll"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient2.Camera.pdb"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
; DOME DRIVERS
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient1.Dome.dll"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient1.Dome.pdb"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient2.Dome.dll"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient2.Dome.pdb"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
; FILTER WHEEL DRIVERS
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient1.FilterWheel.dll"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient1.FilterWheel.pdb"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient2.FilterWheel.dll"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient2.FilterWheel.pdb"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
; FOCUSER DRIVERS
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient1.Focuser.dll"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient1.Focuser.pdb"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient2.Focuser.dll"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient2.Focuser.pdb"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
; OBSERVINGCONDITIONS DRIVERS
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient1.ObservingConditions.dll"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient1.ObservingConditions.pdb"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient2.ObservingConditions.dll"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient2.ObservingConditions.pdb"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
; ROTATOR DRIVERS
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient1.Rotator.dll"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient1.Rotator.pdb"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient2.Rotator.dll"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient2.Rotator.pdb"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
; SAFETYMONITOR DRIVERS
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient1.SafetyMonitor.dll"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient1.SafetyMonitor.pdb"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient2.SafetyMonitor.dll"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient2.SafetyMonitor.pdb"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
; SWITCH DRIVERS
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient1.Switch.dll"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient1.Switch.pdb"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient2.Switch.dll"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient2.Switch.pdb"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
; TELESCOPE DRIVERS
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient1.Telescope.dll"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient1.Telescope.pdb"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient2.Telescope.dll"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
Source: "..\Web Client Local Server\bin\Release\ASCOM.WebClient2.Telescope.pdb"; DestDir: "{app}\WebClient"; Flags: ignoreversion; Components: ClientComponents
; REMOTE SERVER FILES
Source: "..\Remote Device Server\bin\Release\ASCOM.RemoteDeviceServer.exe"; DestDir: "{pf}\ASCOM\RemoteServer"; Flags: ignoreversion; Components: ServerComponents
Source: "..\Remote Device Server\bin\Release\ASCOM.RemoteDeviceServer.pdb"; DestDir: "{pf}\ASCOM\RemoteServer"; Flags: ignoreversion; Components: ServerComponents
; REMOTE SERVER SUPPORT FILES
Source: "..\Remote Device Server\bin\Release\RestSharp.dll"; DestDir: "{pf}\ASCOM\RemoteServer"; Flags: ignoreversion; Components: ServerComponents
Source: "..\Remote Device Server\bin\Release\Newtonsoft.Json.dll"; DestDir: "{pf}\ASCOM\RemoteServer"; Flags: ignoreversion; Components: ServerComponents
Source: "..\Remote Device Server\bin\Release\ASCOM.WebClient.LocalServer.exe"; DestDir: "{pf}\ASCOM\RemoteServer"; Flags: ignoreversion; Components: ServerComponents
Source: "..\Remote Device Server\bin\Release\ASCOM.RemoteDeviceServer.pdb"; DestDir: "{pf}\ASCOM\RemoteServer"; Flags: ignoreversion; Components: ServerComponents
; SET NETWORK PERMISSIONS FILES
Source: "..\SetNetworkPermissions\bin\Release\CommandLine.dll"; DestDir: "{app}\WebClient"; Flags: ignoreversion
Source: "..\SetNetworkPermissions\bin\Release\Interop.NetFwTypeLib.dll"; DestDir: "{app}\WebClient"; Flags: ignoreversion
Source: "..\SetNetworkPermissions\bin\Release\SetNetworkPermissions.exe"; DestDir: "{app}\WebClient"; Flags: ignoreversion
Source: "..\SetNetworkPermissions\bin\Release\SetNetworkPermissions.exe.config"; DestDir: "{app}\WebClient"; Flags: ignoreversion
Source: "..\SetNetworkPermissions\bin\Release\SetNetworkPermissions.pdb"; DestDir: "{app}\WebClient"; Flags: ignoreversion
Source: "..\SetNetworkPermissions\bin\Release\WindowsFirewallHelper.dll"; DestDir: "{app}\WebClient"; Flags: ignoreversion

; WEB CONTENT
;Source: "..\Web Content\index.html"; DestDir: "{pf}\ASCOM\RemoteServer"; Components: WebContent

[Run]
Filename: "{app}\WebClient\ASCOM.WebClient.LocalServer.exe"; Parameters: "/regserver"
Filename: "{app}\WebClient\SetNetworkPermissions.exe"; Parameters: "-l ""{app}\WebClient\ASCOM.WebClient.LocalServer.exe"""; Components: ClientComponents; Flags: runhidden
Filename: "{app}\WebClient\SetNetworkPermissions.exe"; Parameters: "-r ""{pf}\ASCOM\RemoteServer\ASCOM.RemoteDeviceServer.exe"""; Components: ServerComponents; Flags: runhidden
; Filename: "{pf}\ASCOM\RemoteServer\ASCOM.RemoteDeviceServer.exe"; Flags: postinstall

[UninstallRun]
Filename: "{app}\WebClient\ASCOM.WebClient.LocalServer.exe"; Parameters: "/unregserver"

[Registry]

[Icons]
Name: "{group}\ASCOM Remote Driver Server"; Filename: "{pf}\ASCOM\RemoteServer\ASCOM.RemoteDeviceServer.exe"

[Components]
Name: "ClientComponents"; Description: "Client components"; Types: ClientOnly Full;
Name: "ServerComponents"; Description: "Server components"; Types: Full ServerOnly;
;Name: "WebContent"; Description: "Web server content files"; Types: Full ServerOnly;

[Types]
Name: "Full"; Description: "Client and server components"
Name: "ClientOnly"; Description: "Client components only"
Name: "ServerOnly"; Description: "Server components only"
Name: "Custom"; Description: "Custom"; Flags: iscustom

[CODE]
const
   REQUIRED_PLATFORM_VERSION = 6.2;    // Set this to the minimum required ASCOM Platform version for this application

//
// Function to return the ASCOM Platform's version number as a double.
//
// The Platform version number is stored in the Profile using point as the decimal separator but some locales use other
// characters as decimal separators, which means that the Profile version string can't be parsed directly to a double 
// on all systems.
//
// This routine extracts the decimal separator currently in use on the system and converts the point character in the Platform
// version string to this character before parsing the revised string value to a double, which is returned to the caller.
//
// This approach has to be used because Inno 5.5.9 can't create the ASCOM.Utilities.Util COM object and
// so we can't use the Util.IsPlatformVersion function
//
function PlatformVersion(): Double;
var
   PlatVerString : String;
   PlatVer : Variant;
   DoubleValue : Variant;
   DoubleValueString : String;
   Separator : String;
begin
   Result := 0.0;  // Initialise the return value in case we can't read the registry
   try
      if RegQueryStringValue(HKEY_LOCAL_MACHINE, 'Software\ASCOM','PlatformVersion', PlatVerString) then      // Successfully read the string value so convert it to a variant double
      begin
         DoubleValue := 1.0 / 3.0; // Create a real number of value 0.33333
         DoubleValueString := DoubleValue; // Get the real number as a string including this system's decimal separator
         Separator := Copy(DoubleValueString,2,1); // Parse out the decimal separator
         StringChangeEx(PlatVerString, '.', Separator, True); // Change the "." to the current system's decimal separator
         Result := StrToFloat(PlatVerString); // Create a double from the modified string that contains this system's decimal separator
      end;
   except                                                                   
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