@echo off
setlocal

set thisdir=%~dp0
set configuration=Debug

:parseargs
if "%1" == "" goto argsdone
if /i "%1" == "-c" goto set_configuration
if /i "%1" == "--configuration" goto set_configuration

echo Unsupported argument: %1
exit /b 1

:set_configuration
set configuration=%2
shift
shift
goto parseargs

:argsdone

set src=%~dp0..\IxMilia.BCad.Server\IxMilia.BCad.Server.csproj
set dest=%~dp0bin

dotnet restore %src%
dotnet build %src% -c %configuration%
dotnet publish %src% -c %configuration%

robocopy %~dp0..\..\artifacts\bin\IxMilia.BCad.Server\%configuration%\netcoreapp3.1\publish %dest% /s /mir
exit /b 0
