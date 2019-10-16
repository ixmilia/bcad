@echo off

set src=%~dp0..\IxMilia.BCad.Server\IxMilia.BCad.Server.csproj
set dest=%~dp0bin

dotnet restore %src%
dotnet build %src%
dotnet publish %src%

robocopy %~dp0..\..\artifacts\bin\IxMilia.BCad.Server\Debug\netcoreapp3.0\publish %dest% /s /mir
exit /b 0
