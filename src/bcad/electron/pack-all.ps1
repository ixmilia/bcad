#!/usr/bin/pwsh

[CmdletBinding(PositionalBinding=$false)]
param (
    [string]$configuration = "Debug"
)

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

try {
    $platform = if ($IsWindows) { "win32" } else { "linux" }
    $rid = if ($IsWindows) { "win-x64" } else { "linux-x64" }
    npx electron-packager . --platform=$platform --arch=x64 --icon=./out/bcad.ico --extra-resource=../../../artifacts/bin/IxMilia.BCad.Server/$configuration/netcoreapp3.1/$rid/publish/ --out=../../../artifacts/pack --overwrite
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}
