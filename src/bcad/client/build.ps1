#!/usr/bin/pwsh

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

try {
    Push-Location $PSScriptRoot

    npm i
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    npm run compile
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    $htmlContent = Get-Content -Path "index.html" -Raw
    $cssContent = Get-Content -Path "style.css" -Raw
    $jsContent = Get-Content -Path "out/bcad-client.js" -Raw

    $htmlContent = $htmlContent.Replace("/*STYLE-CONTENT*/", $cssContent)
    $htmlContent = $htmlContent.Replace("/*JS-CONTENT*/", $jsContent)

    $htmlContent | Out-File -FilePath "out/index.html"
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
} finally {
    Pop-Location
}
