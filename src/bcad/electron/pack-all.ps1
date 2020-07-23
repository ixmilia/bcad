#!/usr/bin/pwsh

[CmdletBinding(PositionalBinding=$false)]
param (
    [string]$configuration = "Debug"
)

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

try {
    $platform = if ($IsLinux) { "linux" } elseif ($IsMacOS) { "darwin" } elseif ($IsWindows) { "win32" }
    $rid = if ($IsLinux) { "linux-x64" } elseif ($IsMacOS) { "osx-x64" } elseif ($IsWindows) { "win-x64" }
    npx electron-packager . --platform=$platform --arch=x64 --icon=./out/bcad.ico --extra-resource=../../../artifacts/bin/IxMilia.BCad.Server/$configuration/netcoreapp3.1/$rid/publish/ --out=../../../artifacts/pack --overwrite
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}
