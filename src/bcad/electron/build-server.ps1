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
    dotnet publish "$projectPath" -c $configuration -f $tfm -r $rid -p:PublishSingleFile=true

    # enable debugging by copying this file to the output directory
    $ridDir = "$PSScriptRoot/../../../artifacts/bin/IxMilia.BCad.Server/$configuration/$tfm/$rid"
    if (Test-Path "$ridDir/mscordbi.dll") {
        Copy-Item "$ridDir/mscordbi.dll" "$ridDir/publish/"
    }
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}
