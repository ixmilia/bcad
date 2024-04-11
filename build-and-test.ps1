#!/usr/bin/pwsh

[CmdletBinding(PositionalBinding = $false)]
param (
    [string][Alias('c')]$configuration = "Debug",
    [string][Alias('a')]$architecture,
    [switch]$noTest,
    [string]$deployTo
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
    $runTests = -Not $noTest

    # assign architecture
    if ($architecture -eq '') {
        if ($IsWindows) {
            $architecture = if ($env:PROCESSOR_ARCHITECTURE -eq "AMD64") { "x64" } else { "arm64" }
        }
        else {
            $architecture = if ((arch | Out-String).Trim() -eq "x86_64") { "x64" } else { "arm64" }
        }
    }

    # build submodule
    Push-Location "$PSScriptRoot/src/IxMilia.Converters"
    . .\build-and-test.ps1 -configuration $configuration -noTest
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Pop-Location

    # restore
    dotnet restore
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    # build client contracts file
    . ./build-contracts-files.ps1
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    # build js client
    Push-Location "$PSScriptRoot/src/javascript-client"
    npm i
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    if ($IsLinux -And $architecture -eq "arm64") {
        # when cross-compiling for linux arm64, we specifically need the x64 version of icon-gen
        npm install --platform=linux --arch=x64 icon-gen
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }
    npm run compile
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Pop-Location

    # build
    dotnet build --configuration $configuration --no-restore
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    # tests
    if ($runTests) {
        dotnet test --no-restore --no-build --configuration $configuration
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }

    # publish
    $os = if ($IsLinux) { "linux" } elseif ($IsMacOS) { "osx" } elseif ($IsWindows) { "win" }
    $packagesDir = "$PSScriptRoot/artifacts/packages/$configuration"
    New-Item -ItemType Directory -Path $packagesDir -Force
    $packageParentDir = "$PSScriptRoot/artifacts/publish/$configuration"
    $packageOutputDir = "$packageParentDir/bcad-$os-$architecture"

    . ./publish.ps1 -configuration $configuration -runtime "$os-$architecture" -output $packageOutputDir
    if ($IsLinux -And ($architecture -eq "arm64") -And ($LASTEXITCODE -ne 0)) {
        # this never works the first time?
        . ./publish.ps1 -configuration $configuration -runtime "$os-$architecture" -output $packageOutputDir
    }
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    # remove unnecessary files
    $unnecessaryFiles = @(
        "bcad.dbg",
        "bcad.pdb",
        "IxMilia.Dxf.xml"
    )
    foreach ($file in $unnecessaryFiles) {
        if (Test-Path "$packageOutputDir/$file") {
            Remove-Item "$packageOutputDir/$file"
        }
    }

    # create bccoreconsole
    Push-Location "$PSScriptRoot/src/bccoreconsole"
    $goarch = if ($architecture -eq "x64") { "amd64" } else { "arm64" }
    $env:GOARCH = $goarch
    go build -o "$packageOutputDir/" -buildvcs=false
    $env:GOARCH = ""
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Pop-Location

    # create package
    $artifactShortName = "bcad-$os-$architecture"
    $artifactExtension = if ($IsWindows) { "zip" } else { "tar.gz" }
    $artifactName = "$artifactShortName.$artifactExtension"
    $artifactPath = "$packagesDir/$artifactName"
    Set-EnvironmentVariable "artifact_name" $artifactName
    Set-EnvironmentVariable "artifact_path" $artifactPath
    if ($IsWindows) {
        Compress-Archive -Path "$packageOutputDir" -DestinationPath $artifactPath -Force
        Set-EnvironmentVariable "secondary_artifact_name" "win-$architecture"
        Set-EnvironmentVariable "secondary_artifact_path" $artifactPath
    }
    else {
        $packageVersionPrefix = (Get-Content "$PSScriptRoot/version.txt" | Out-String).Trim()
        $packageVersionSuffix = if ("$env:VERSION_SUFFIX" -eq "") { "0" } else { $env:VERSION_SUFFIX }
        $packageVersion = "$packageVersionPrefix.$packageVersionSuffix"
        $packageArchitecture = if ($architecture -eq "x64") { "amd64" } else { "arm64" }
        $packageName = "bcad_${packageVersion}_$packageArchitecture.deb"
        Set-EnvironmentVariable "secondary_artifact_name" "deb-$architecture"
        Set-EnvironmentVariable "secondary_artifact_path" "$packagesDir/$packageName"
        tar -zcf $artifactPath -C "$packageParentDir/" "$artifactShortName"
        ./build-package.sh --configuration $configuration --architecture $architecture
    }

    if ($deployTo -ne "") {
        $deploySource = "$packageParentDir/$artifactShortName"
        $deployDestination = "$deployTo/$artifactShortName"

        if (Test-Path $deployDestination) {
            Remove-Item -Path $deployDestination -Recurse -Force
        }

        Write-Host "Deploying to $deployDestination"
        Copy-Item -Path $deploySource -Destination $deployDestination -Recurse -Force
    }
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}
