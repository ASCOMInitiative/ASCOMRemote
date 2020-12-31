@echo on
@echo Setting up variables
call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvarsall.bat" x86

@echo Parameter: %1

echo "%1" | findstr /C:"uninst" 1>nul

if errorlevel 1 (
	echo got errorlevel one -  pattern missing
) ELSE (
	echo got errorlevel zero - found pattern

	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Client Local Server\bin\Debug\ASCOM.RemoteClientLocalServer.exe"
	signtool sign /a /as /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Client Local Server\bin\Debug\ASCOM.RemoteClientLocalServer.exe"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\ASCOM.DynamicRemoteClients\bin\Debug\ASCOM.DynamicRemoteClients.exe"
	signtool sign /a /as /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\ASCOM.DynamicRemoteClients\bin\Debug\ASCOM.DynamicRemoteClients.exe"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Client Device Base Classes\bin\Debug\ASCOM.RemoteClientBaseClasses.dll"
	signtool sign /a /as /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Client Device Base Classes\bin\Debug\ASCOM.RemoteClientBaseClasses.dll"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Client Local Server\bin\Debug\RestSharp.dll"
	signtool sign /a /as /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Client Local Server\bin\Debug\RestSharp.dll"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Server\bin\Debug\ASCOM.RemoteServer.exe"
	signtool sign /a /as /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Server\bin\Debug\ASCOM.RemoteServer.exe"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Debug\ASCOM.SetNetworkPermissions.exe"
	signtool sign /a /as /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Debug\ASCOM.SetNetworkPermissions.exe"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Debug\WindowsFirewallHelper.dll"
	signtool sign /a /as /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Debug\WindowsFirewallHelper.dll"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Debug\CommandLine.dll"
	signtool sign /a /as /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Debug\CommandLine.dll"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Debug\NetStandard.dll"
	signtool sign /a /as /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Debug\NetStandard.dll"

	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Client Local Server\bin\Release\ASCOM.RemoteClientLocalServer.exe"
	signtool sign /a /as /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Client Local Server\bin\Release\ASCOM.RemoteClientLocalServer.exe"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\ASCOM.DynamicRemoteClients\bin\Release\ASCOM.DynamicRemoteClients.exe"
	signtool sign /a /as /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\ASCOM.DynamicRemoteClients\bin\Release\ASCOM.DynamicRemoteClients.exe"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Client Device Base Classes\bin\Release\ASCOM.RemoteClientBaseClasses.dll"
	signtool sign /a /as /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Client Device Base Classes\bin\Release\ASCOM.RemoteClientBaseClasses.dll"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Client Local Server\bin\Release\RestSharp.dll"
	signtool sign /a /as /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Client Local Server\bin\Release\RestSharp.dll"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Server\bin\Release\ASCOM.RemoteServer.exe"
	signtool sign /a /as /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\Remote Server\bin\Release\ASCOM.RemoteServer.exe"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Release\ASCOM.SetNetworkPermissions.exe"
	signtool sign /a /as /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Release\ASCOM.SetNetworkPermissions.exe"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Release\WindowsFirewallHelper.dll"
	signtool sign /a /as /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Release\WindowsFirewallHelper.dll"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Release\CommandLine.dll"
	signtool sign /a /as /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Release\CommandLine.dll"
	signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Release\NetStandard.dll"
	signtool sign /a /as /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 "J:\ASCOMRemote\SetNetworkPermissions\bin\Release\NetStandard.dll"

)

echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 %1
signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 %1
signtool sign /a /as /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 %1
