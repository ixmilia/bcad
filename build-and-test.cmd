@echo off

set coretestproject=%~dp0src\IxMilia.BCad.Core.Test\IxMilia.BCad.Core.Test.csproj
set filehandlerstestproject=%~dp0src\IxMilia.BCad.FileHandlers.Test\IxMilia.BCad.FileHandlers.Test.csproj

:: IxMilia.Dxf needs a custom invocation
call %~dp0src\IxMilia.Dxf\build-and-test.cmd -notest
if errorlevel 1 echo Error pre-building IxMilia.Dxf && goto error

:: restore packages
dotnet restore
if errorlevel 1 echo Error restoring packages && goto error

:: build .NET
dotnet build
if errorlevel 1 echo Error building solution && goto error

:: test
dotnet test --no-restore --no-build "%coretestproject%"
if errorlevel 1 echo Error running tests && goto error
dotnet test --no-restore --no-build "%filehandlerstestproject%"
if errorlevel 1 echo Error running tests && goto error

:: build electron
pushd %~dp0src\bcad
call npm i
if errorlevel 1 echo Error restoring npm packages && goto error
call npm run pack
if errorlevel 1 echo Error packing electron && goto error
popd

exit /b 0

:error
echo Build exited with failures.
cd /d %~dp0
exit /b 1
