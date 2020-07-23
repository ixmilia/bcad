#!/usr/bin/pwsh

[CmdletBinding(PositionalBinding=$false)]
param (
    [string]$version = "42.42.42"
)

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

try {
    Push-Location $PSScriptRoot

    npm i
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    npm version $version --allow-same-version
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    npm run package
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    npm version 42.42.42 --allow-same-version
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
} finally {
    Pop-Location
}
