#!/usr/bin/pwsh

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

try {
    # build client contracts file
    $interfaceGeneratorProject = "$PSScriptRoot/src/IxMilia.BCad.InterfaceGenerator/IxMilia.BCad.InterfaceGenerator.csproj"
    $contractsFiles = @(
        "$PSScriptRoot/src/javascript-client/src/contracts.generated.ts"
    )
    dotnet build $interfaceGeneratorProject
    dotnet run --project $interfaceGeneratorProject -- $contractsFiles
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    # generate code for lisp
    Push-Location "$PSScriptRoot/src/IxMilia.Lisp"
    . .\generate-code.ps1
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Pop-Location
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}
