@echo on
call "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat" -startdir=none -arch=x64 -host_arch=x64

"C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x86\signtool" sign /a /fd SHA256 /n "Peter Simpson" /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 %1




rem @echo on
rem @echo Setting up variables
rem call "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvarsall.bat" x86
rem @echo " " 
rem @echo Parameter: %1

rem echo "%1" | findstr /C:"uninst" 1>nul

rem We only need to sign these executables once, so do this when the uninstaller is created.
rem if errorlevel 1 (
	rem echo Signing main installer - Not signing other executables 
rem ) ELSE (
	rem echo Signing uninstaller - Signing other exectutables too.

	rem echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Server\bin\Debug\ASCOM.RemoteServer.exe"
	rem echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Server\bin\Debug\ASCOM.Common.dll"
	rem echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Debug\ASCOM.SetNetworkPermissions.exe"
	rem echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Debug\WindowsFirewallHelper.dll"
	rem echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Debug\CommandLine.dll"
	rem echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Debug\NetStandard.dll"

	rem signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Server\bin\Debug\ASCOM.RemoteServer.exe"
	rem signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Server\bin\Debug\ASCOM.Common.dll"
	rem signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Debug\ASCOM.SetNetworkPermissions.exe"
	rem signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Debug\WindowsFirewallHelper.dll"
	rem signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Debug\CommandLine.dll"
	rem signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Debug\NetStandard.dll"

	rem echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Server\bin\Release\ASCOM.RemoteServer.exe"
	rem echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Server\bin\Release\ASCOM.Common.dll"
	rem echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Release\ASCOM.SetNetworkPermissions.exe"
	rem echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Release\WindowsFirewallHelper.dll"
	rem echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Release\CommandLine.dll"
	rem echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Release\NetStandard.dll"

	rem signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Server\bin\Release\ASCOM.RemoteServer.exe"
	rem signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Server\bin\Release\ASCOM.Common.dll"
	rem signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Release\ASCOM.SetNetworkPermissions.exe"
	rem signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Release\WindowsFirewallHelper.dll"
	rem signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Release\CommandLine.dll"
	rem signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Release\NetStandard.dll"

)

rem Wait for 1 second to allow Inno to release a file handle, which prevents signtool from working correctly.
rem timeout 1

rem Sign the installer or uninstaller whose filename was supplied as the first parameter
rem echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 %1
rem signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 %1

rem pause
