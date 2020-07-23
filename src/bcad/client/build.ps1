[CmdletBinding(PositionalBinding=$false)]
param (
    [string]$version = "42.42.42"
)

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

try {
    Push-Location $PSScriptRoot

    npm i

    npm version $version --allow-same-version

    npm run compile

    npm version 42.42.42 --allow-same-version

    $htmlContent = Get-Content -Path "index.html" -Raw
    $cssContent = Get-Content -Path "style.css" -Raw
    $jsContent = Get-Content -Path "out\bcad-client.js" -Raw

    $htmlContent = $htmlContent.Replace("/*STYLE-CONTENT*/", $cssContent)
    $htmlContent = $htmlContent.Replace("/*JS-CONTENT*/", $jsContent)

    $htmlContent | Out-File -FilePath "out\index.html"
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
} finally {
    Pop-Location
}
