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
if errorlevel 1 echo Error pre-building IxMilia.Dxf && exit /b 1

:: restore packages
dotnet restore "%srcdir%\BCad\BCad.csproj"
if errorlevel 1 echo Error restoring packages && exit /b 1
dotnet restore "%toplevelproject%"
if errorlevel 1 echo Error restoring packages && exit /b 1

:: build
msbuild "%slnfile%"
if errorlevel 1 echo Error building solution && exit /b 1

:: test
dotnet test "%coretestproject%"
if errorlevel 1 echo Error running tests && exit /b 1
dotnet test "%filehandlerstestproject%"
if errorlevel 1 echo Error running tests && exit /b 1
