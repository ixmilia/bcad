@echo off
set srcdir=%~dp0src
set slnfile=%srcdir%\BCad.sln

set filehandlerstestproject=%srcdir%\IxMilia.BCad.FileHandlers.Test\IxMilia.BCad.FileHandlers.Test.csproj
set coretestproject=%srcdir%\IxMilia.BCad.Core.Test\IxMilia.BCad.Core.Test.csproj

:: every dotnet core project is eventually referenced off of this one
set toplevelproject=%filehandlerstestproject%

:: restore packages
dotnet restore "%srcdir%\BCad\BCad.csproj"
dotnet restore "%toplevelproject"

:: build
dotnet build "%toplevelproject%"
msbuild "%slnfile%"

:: test
dotnet test "%coretestproject%"
dotnet test "%filehandlerstestproject%"
