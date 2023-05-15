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
dotnet publish "remote server\remote server.csproj" --runtime win-x86 --self-contained -p:publishsinglefile=true -o publish\x86
@echo Publishing ASCOM Remote x64
dotnet publish "remote server\remote server.csproj" --runtime win-x64 --self-contained -p:publishsinglefile=true -o publish\x64