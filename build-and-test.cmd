@echo off

set configuration=Debug
set runtests=true

:parseargs
if "%1" == "" goto argsdone
if /i "%1" == "-c" goto set_configuration
if /i "%1" == "--configuration" goto set_configuration
if /i "%1" == "-notest" goto set_notest
if /i "%1" == "--notest" goto set_notest

echo Unsupported argument: %1
goto error

:set_configuration
set configuration=%2
shift
shift
goto parseargs

:set_notest
set runtests=false
shift
goto parseargs

:argsdone

:: IxMilia.Dxf needs a custom invocation
call %~dp0src\IxMilia.Dxf\build-and-test.cmd -c %configuration% -notest -portable
if errorlevel 1 echo Error pre-building IxMilia.Dxf && goto error

:: restore packages
dotnet restore
if errorlevel 1 echo Error restoring packages && goto error

:: build .NET
dotnet build -c %configuration%
if errorlevel 1 echo Error building solution && goto error

:: test
if /i "%runtests%" == "true" (
    dotnet test --no-restore --no-build -c %configuration%
    if errorlevel 1 echo Error running tests && goto error
)

exit /b 0

:error
echo Build exited with failures.
cd /d %~dp0
exit /b 1
