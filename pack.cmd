@echo off

set configuration=Debug

:parseargs
if "%1" == "" goto argsdone
if /i "%1" == "-c" goto set_configuration
if /i "%1" == "--configuration" goto set_configuration

echo Unsupported argument: %1
goto error

:set_configuration
set configuration=%2
shift
shift
goto parseargs

:argsdone

mkdir "%~dp0artifacts\publish"

:: compress server binaries
set /p package_version=<version.txt
set server_filename=IxMilia.BCad.Server-%package_version%.zip
powershell -Command "Compress-Archive -Path '%~dp0artifacts\bin\IxMilia.BCad.Server\%configuration%\netcoreapp3.1' -DestinationPath '%~dp0artifacts\publish\%server_filename%' -Force"
if errorlevel 1 echo Error creating server archive && goto error

:: build and pack VS Code extension
set extension_directory=%~dp0src\bcad-vscode
pushd "%extension_directory%"
set extension_filename=bcad-%package_version%.vsix
call build.cmd --version %package_version%
if errorlevel 1 echo Error building VS Code extension && goto error
popd
copy "%extension_directory%\%extension_filename%" "%~dp0artifacts\publish\"

:: report final artifact names for GitHub Actions
echo ::set-env name=extension_artifact_name::%extension_filename%
echo ::set-env name=server_artifact_name::%server_filename%

exit /b 0

:error
echo Build exited with failures.
cd /d %~dp0
exit /b 1
