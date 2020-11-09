@echo on
@echo Setting up variables
call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvarsall.bat" x86

@echo Parameter: %1

echo signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 %1
signtool sign /a /fd SHA256 /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 %1
signtool sign /a /as /tr http://rfc3161timestamp.globalsign.com/advanced /td SHA256 %1