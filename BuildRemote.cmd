@echo on
@echo Setting up variables
call "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvarsall.bat" x86
@echo Selecting Remote directory

SET RemoteDirectory=%cd%
@echo Current directory: %RemoteDirectory%

@echo Restoring packages
msbuild /t:restore /p:configuration=Debug;Platform="Any CPU"

@echo Building Debug AnyCPU in directory %cd%
msbuild /t:rebuild /p:configuration=Debug;Platform="Any CPU"

cd %RemoteDirectory%
@echo Building Release AnyCPU in directory %cd%
msbuild /t:rebuild /p:configuration=Release;Platform="Any CPU"

cd %RemoteDirectory%
rmdir /s /q "publish"
@echo Publishing ASCOM Remote x86
dotnet publish "remote server\remote server.csproj" --runtime win-x86 --self-contained -p:Configuration=Debug -p:Platform="x86" -p:publishsinglefile=true -o publish\remote\x86\

@echo Publishing ASCOM Remote x64
dotnet publish "remote server\remote server.csproj" --runtime win-x64 --self-contained -p:Configuration=Debug -p:Platform="Any CPU" -p:publishsinglefile=true -o publish\remote\x64\

@echo Publishing Set Network Permissions x86
dotnet publish "SetNetworkPermissions\SetNetworkPermissions.csproj" --runtime win-x86 --self-contained -p:Configuration=Debug -p:Platform="x86" -p:publishsinglefile=true -o publish\permissions\x86\

rem @echo Publishing Set Network Permissions x64
rem dotnet publish "SetNetworkPermissions\SetNetworkPermissions.csproj" --runtime win-x64 --self-contained -p:Configuration=Debug -p:Platform="Any CPU" -p:publishsinglefile=true -o publish\permissions\x64\

rem @echo Publishing ASCOM Remote x86 - Needs Support Library
rem dotnet publish "remote server\remote server.csproj" --runtime win-x86 -p:Configuration=Debug --self-contained false -p:Platform="x86" -p:publishsinglefile=true -o publish\x86NeedsCoreInstall\
rem @echo Publishing ASCOM Remote x64 - Needs Support Library
rem dotnet publish "remote server\remote server.csproj" --runtime win-x64 -p:Configuration=Debug --self-contained false -p:Platform="Any CPU" -p:publishsinglefile=true -o publish\x64NeedsCoreInstall\

@echo *** Creating Windows installer
cd Setup
"C:\Program Files (x86)\Inno Script Studio\isstudio.exe" -compile "ASCOM Remote Setup.iss"
cd ..
@echo *** Finsihed!
pause