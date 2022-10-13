@echo on
@echo Setting up variables
call "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvarsall.bat" x86
@echo " " 
@echo Parameter: %1

echo "%1" | findstr /C:"uninst" 1>nul

rem We only need to sign these executables once, so do this when the uninstaller is created.
if errorlevel 1 (
	echo Signing main installer - Not signing other executables 
) ELSE (
	echo Signing uninstaller - Signing other exectutables too.

	echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Server\bin\Debug\ASCOM.RemoteServer.exe"
	echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Server\bin\Debug\ASCOM.Common.dll"
	echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Debug\ASCOM.SetNetworkPermissions.exe"
	echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Debug\WindowsFirewallHelper.dll"
	echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Debug\CommandLine.dll"
	echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Debug\NetStandard.dll"

	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Server\bin\Debug\ASCOM.RemoteServer.exe"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Server\bin\Debug\ASCOM.Common.dll"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Debug\ASCOM.SetNetworkPermissions.exe"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Debug\WindowsFirewallHelper.dll"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Debug\CommandLine.dll"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Debug\NetStandard.dll"

	echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Server\bin\Release\ASCOM.RemoteServer.exe"
	echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Server\bin\Release\ASCOM.Common.dll"
	echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Release\ASCOM.SetNetworkPermissions.exe"
	echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Release\WindowsFirewallHelper.dll"
	echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Release\CommandLine.dll"
	echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Release\NetStandard.dll"

	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Server\bin\Release\ASCOM.RemoteServer.exe"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Server\bin\Release\ASCOM.Common.dll"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Release\ASCOM.SetNetworkPermissions.exe"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Release\WindowsFirewallHelper.dll"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Release\CommandLine.dll"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Release\NetStandard.dll"

)

rem Wait for 1 second to allow Inno to release a file handle, which prevents signtool from working correctly.
timeout 1

rem Sign the installer or uninstaller whose filename was supplied as the first parameter
echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 %1
signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 %1

pause
