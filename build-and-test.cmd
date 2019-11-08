@echo off
setlocal

set configuration=Debug

:parseargs
if "%1" == "" goto argsdone
if /i "%1" == "-c" (
    set configuration=%2
    shift
    shift
    goto parseargs
)

echo Unsupported argument: %1
goto error

:argsdone

:: IxMilia.Dxf needs a custom invocation to generate code
call %~dp0src\IxMilia.Dxf\build-and-test.cmd -notest -c %configuration%
if errorlevel 1 echo Error pre-building IxMilia.Dxf && exit /b 1

:: build
set SOLUTION=%~dp0src\BCad.sln
msbuild "%SOLUTION%" /t:Restore
if errorlevel 1 exit /b 1
msbuild "%SOLUTION%" /t:Build /p:Configuration=%configuration%
if errorlevel 1 exit /b 1

:: test
dotnet test "%SOLUTION%" -c %configuration% --no-restore --no-build
if errorlevel 1 exit /b 1
