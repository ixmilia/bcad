#!/usr/bin/pwsh

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

try {
    $sourceDir = "$PSScriptRoot/../client/out"
    $destinationDir = "$PSScriptRoot/out/client"

    # copy client compiled directory
    if (Test-Path $destinationDir -PathType Container) {
        Remove-Item -Path $destinationDir -Recurse -Force
    }
    Copy-Item -Path $sourceDir -Destination $destinationDir -Exclude @("pack/*") -Recurse -Force

    # copy client src directory
    if (Test-Path "$destinationDir/../src" -PathType Container) {
        Remove-Item -Path "$destinationDir/../src" -Recurse -Force
    }
    Copy-Item -Path "$sourceDir/../src" -Destination "$destinationDir/../src" -Recurse -Force

    # copy css
    Copy-Item -Path "$sourceDir/../style.css" -Destination $destinationDir -Force
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}
