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

    # create packed file
    $packedContent = $htmlContent
    $packedContent = $packedContent.Replace("<!--STYLE-CONTENT-->", "<style>$cssContent</style>")
    $packedContent = $packedContent.Replace("<!--JS-CONTENT-->", "<script type=""text/javascript"">$jsContent</script>")

    if (!(Test-Path "out/pack" -PathType Container)) {
        New-Item -ItemType Directory -Force -Path "out/pack"
    }
    $packedContent | Out-File -FilePath "out/pack/index.html"

    # create relative file
    $relativeContent = $htmlContent
    $relativeContent = $relativeContent.Replace("<!--STYLE-CONTENT-->", "<link rel=""stylesheet"" href=""style.css"" />")
    $relativeContent = $relativeContent.Replace("<!--JS-CONTENT-->", "<script type=""text/javascript"">var exports = {};</script><script type=""text/javascript"" src=""main.js""></script>")
    $relativeContent | Out-File -FilePath "out/index.html"
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
} finally {
    Pop-Location
}
