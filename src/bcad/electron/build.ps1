#!/usr/bin/pwsh

[CmdletBinding(PositionalBinding=$false)]
param (
    [string]$configuration = "Debug",
    [string]$version = "42.42.42"
)

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

function Set-EnvironmentVariable([string]$name, [string]$value) {
    Write-Host "setting $name=$value"
    if (Test-Path env:GITHUB_ENV) {
        Write-Output "$name=$value" | Out-File -FilePath $env:GITHUB_ENV -Encoding utf8 -Append
    }
}

try {
    Push-Location $PSScriptRoot

    npm i
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    npm version $version --allow-same-version
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    npm config set bcad:configuration $configuration && npm run pack
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    npm version 42.42.42 --allow-same-version
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    $repoRoot = "$PSScriptRoot/../../.."
    New-Item -ItemType Directory -Path "$repoRoot/artifacts/publish" -Force
    $suffix = ""
    if ($configuration -eq "Debug") { $suffix = "-debug" }
    $os = if ($IsLinux) { "linux" } elseif ($IsMacOS) { "darwin" } elseif ($IsWindows) { "win32" }
    $extension = if ($IsWindows) { "zip" } else { "tar.gz" }
    $filename = "bcad-$os-x64$suffix-$version.$extension"

    # report final artifact names for GitHub Actions
    Set-EnvironmentVariable "electron_artifact_file_name" $filename

    # and finally compress
    if ($IsWindows) {
        Compress-Archive -Path "$repoRoot/artifacts/pack/bcad-$os-x64/" -DestinationPath "$repoRoot/artifacts/publish/$filename" -Force
    } else {
        tar -zcf "$repoRoot/artifacts/publish/$filename" -C "$repoRoot/artifacts/pack/" bcad-$os-x64
    }
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
} finally {
    Pop-Location
}
