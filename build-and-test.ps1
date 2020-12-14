#!/usr/bin/pwsh

[CmdletBinding(PositionalBinding=$false)]
param (
    [string]$configuration = "Debug",
    [switch]$noTest
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
    $version = Get-Content -Path "$PSScriptRoot/version.txt"

    # IxMilia.Dxf needs a custom invocation
    $shellExt = if ($IsWindows) { "cmd" } else { "sh" }
    & "$PSScriptRoot/src/IxMilia.Dxf/build-and-test.$shellExt" --configuration $configuration --notest
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    # restore and build this repo
    dotnet restore
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    dotnet build -c $configuration
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    dotnet pack -c $configuration
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    # tests
    if ($noTest) {
        dotnet test --no-restore --no-build -c $configuration
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }

    # build client contracts file
    $contractsFiles = @(
        "$PSScriptRoot/src/bcad/client/src/contracts.generated.ts",
        "$PSScriptRoot/src/bcad/electron/src/contracts.generated.ts",
        "$PSScriptRoot/src/bcad/vscode/src/contracts.generated.ts"
    )
    $runArgs = @()
    foreach ($contractFile in $contractsFiles) {
        $runArgs += "--out-file"
        $runArgs += $contractFile
    }
    dotnet run -p "$PSScriptRoot/src/IxMilia.BCad.Server/IxMilia.BCad.Server.csproj" -- $runArgs
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    # build client
    . $PSScriptRoot/src/bcad/client/build.ps1
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    # build electron
    . $PSScriptRoot/src/bcad/electron/build.ps1 -configuration $configuration -version $version
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    # build VS Code extension
    . $PSScriptRoot/src/bcad/vscode/build.ps1 -version $version
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    # report final artifact names for GitHub Actions
    Set-EnvironmentVariable "global_tool_artifact_file_name" "IxMilia.BCad.Server.$version.nupkg"
    Set-EnvironmentVariable "vscode_artifact_file_name" "bcad-$version.vsix"
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}
