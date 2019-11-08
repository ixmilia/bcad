@echo off
set deploytype=desktop
set deploydir=%1
if [%deploydir%]==[] goto baddir
goto gooddir

:baddir
echo Usage: %0 destination
exit /b 1

:gooddir
set configuration=%2
if [%configuration%]==[] set configuration=Debug
set targetframework=%3
if [%targetframework%]==[] set targetframework=net462

if not exist "%deploydir%" mkdir "%deploydir%"
for /F "tokens=*" %%f in (%~dp0\deployment-files-%deploytype%.txt) do copy "%~dp0..\artifacts\bin\BCad\%configuration%\%targetframework%\%%f" "%deploydir%"
