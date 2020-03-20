@echo off
set srcdir=%~dp0src
set slnfile=%srcdir%\BCad.sln

:: ensure msbuild is on the path
where msbuild > NUL 2>&1
if errorlevel 1 set PATH=%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin;%PATH%

set filehandlerstestproject=%srcdir%\IxMilia.BCad.FileHandlers.Test\IxMilia.BCad.FileHandlers.Test.csproj
set coretestproject=%srcdir%\IxMilia.BCad.Core.Test\IxMilia.BCad.Core.Test.csproj

:: every dotnet core project is eventually referenced off of this one
set toplevelproject=%filehandlerstestproject%

:: IxMilia.Dxf needs a custom invocation
call %~dp0src\IxMilia.Dxf\build-and-test.cmd -notest
if errorlevel 1 echo Error pre-building IxMilia.Dxf && goto error

:: restore packages
dotnet restore "%srcdir%\BCad\BCad.csproj"
if errorlevel 1 echo Error restoring packages && goto error
dotnet restore "%toplevelproject%"
if errorlevel 1 echo Error restoring packages && goto error

:: build wpf
msbuild "%slnfile%"
if errorlevel 1 echo Error building solution && goto error

:: build electron
pushd %srcdir%\BCad.Electron
call npm i
if errorlevel 1 echo Error restoring npm packages && goto error
call npm run pack
if errorlevel 1 echo Error packing electron && goto error
popd

:: test
dotnet test "%coretestproject%"
if errorlevel 1 echo Error running tests && goto error
dotnet test "%filehandlerstestproject%"
if errorlevel 1 echo Error running tests && goto error

exit /b 0

:error
echo Build exited with failures.
cd /d %~dp0
exit /b 1
