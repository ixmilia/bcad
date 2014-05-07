@echo off
set deploytype=desktop
set deploydir=%1
if [%deploydir%]==[] then goto baddir
goto gooddir

:baddir
echo Usage: %0 destination
exit /b 1

:gooddir
if not exist "%deploydir%" mkdir "%deploydir%"
for /F "tokens=*" %%f in (%~dp0\deploy-%deploytype%.txt) do copy "bin\Debug\%%f" "%deploydir%"
