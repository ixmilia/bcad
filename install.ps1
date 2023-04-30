Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

try {
    $architecture = if ($env:PROCESSOR_ARCHITECTURE -eq "AMD64") { "x64" } else { "arm64" }
    $packageName = "bcad-win-$architecture"
    $archiveName = "$packageName.zip"
    $downloadUrl = "https://pkgs.ixmilia.com/bcad/win/$archiveName"
    $downloadPath = Join-Path $env:TEMP $archiveName

    $programsDir = Join-Path $env:LOCALAPPDATA "Programs"
    $installDir = Join-Path $programsDir $packageName

    # remove existing installation
    if (Test-Path $installDir) {
        $deleteExisting = Read-Host "Existing installation found, delete it?  [Y/N]"
        if ($deleteExisting.ToUpper() -ne "Y") {
            exit 0
        }

        Remove-Item $installDir -Recurse -Force
    }

    # download
    if (Test-Path $downloadPath) {
        Remove-Item $downloadPath -Force
    }

    Invoke-WebRequest $downloadUrl -OutFile $downloadPath

    # extract
    Expand-Archive -Path $downloadPath -DestinationPath $programsDir

    # remove archive
    Remove-Item $downloadPath -Force

    # launch
    if ("$env:BCAD_INSTALL_QUIET" -eq "") {
        $exePath = Join-Path $installDir "bcad.exe"
        Start-Process $exePath
    }
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}
