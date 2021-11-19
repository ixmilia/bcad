#!/usr/bin/pwsh

[CmdletBinding(PositionalBinding = $false)]
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
    dotnet restore
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    # build client contracts file
    $interfaceGeneratorProject = "$PSScriptRoot/src/IxMilia.BCad.InterfaceGenerator/IxMilia.BCad.InterfaceGenerator.csproj"
    $contractsFiles = @(
        "$PSScriptRoot/src/javascript-client/src/contracts.generated.ts"
    )
    dotnet build $interfaceGeneratorProject
    dotnet run --project $interfaceGeneratorProject -- $contractsFiles
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    # build js client
    Push-Location "$PSScriptRoot/src/javascript-client"
    npm i
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    npm run compile
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Pop-Location

    # build
    dotnet build --configuration $configuration --no-restore
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    # tests
    if (-Not $noTest) {
        dotnet test --no-restore --no-build --configuration $configuration
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }

    # publish
    $os = if ($IsLinux) { "linux" } elseif ($IsMacOS) { "osx" } elseif ($IsWindows) { "win" }
    foreach ($arch in @("x64", "arm64")) {
        $packageParentDir = "$PSScriptRoot/artifacts/publish/$configuration"
        $packageOutputDir = "$packageParentDir/bcad-$os-$arch"
        dotnet publish "$PSScriptRoot/src/bcad/bcad.csproj" `
            --configuration $configuration `
            --runtime "$os-$arch" `
            --self-contained `
            --output $packageOutputDir
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

        # create package
        $extension = if ($IsWindows) { "zip" } else { "tar.gz" }
        $artifactName = "bcad-$os-$arch.$extension"
        $packagesDir = "$PSScriptRoot/artifacts/packages"
        $fullArtifactPath = "$packagesDir/$artifactName"
        New-Item -ItemType Directory -Path $packagesDir -Force
        Set-EnvironmentVariable "artifact_name_$arch" $artifactName
        Set-EnvironmentVariable "full_artifact_path_$arch" $fullArtifactPath
        if ($IsWindows) {
            Compress-Archive -Path "$packageOutputDir" -DestinationPath $fullArtifactPath -Force
        }
        else {
            tar -zcf "$packagesDir/$artifactName" -C "$packageParentDir/" "bcad-$os-$arch"
        }
    }
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}
