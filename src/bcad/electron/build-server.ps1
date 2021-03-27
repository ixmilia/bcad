#!/usr/bin/pwsh

[CmdletBinding(PositionalBinding = $false)]
param (
    [string]$configuration = "Debug"
)

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

try {
    $projectPath = "$PSScriptRoot/../../IxMilia.BCad.Server/IxMilia.BCad.Server.csproj"
    $tfm = "net5.0"
    $rid = if ($IsLinux) { "linux-x64" } elseif ($IsMacOS) { "osx-x64" } elseif ($IsWindows) { "win-x64" }

    dotnet restore "$projectPath"
    dotnet build "$projectPath" -c $configuration
    dotnet publish "$projectPath" -c $configuration -f $tfm -r $rid
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}
