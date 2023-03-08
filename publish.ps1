#!/usr/bin/pwsh

[CmdletBinding(PositionalBinding = $false)]
param (
    [string]$configuration = "Debug",
    [string]$runtime,
    [string]$output
)

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

try {
    dotnet publish "$PSScriptRoot/src/bcad/bcad.csproj" `
        --configuration $configuration `
        --runtime $runtime `
        --self-contained `
        --output $output
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}
