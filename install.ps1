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

    # wait for process to exit
    $currentProcessId = "$env:BCAD_CURRENT_PROCESS_ID"
    if ($currentProcessId -ne "") {
        # wait for process to exit
        Write-Host "Waiting for process $currentProcessId to exit..."
        try {
            $currentProcess = Get-Process -Id $currentProcessId
            $waitTimeout = 10 * 1000 # 10s
            $hasExited = $currentProcess.WaitForExit($waitTimeout)
            if (-Not $hasExited) {
                # process didn't exit on time, just quit
                Write-Host "Process did not exit"
                exit 1
            }
        }
        catch {
            # process not found, must have already exited
        }
    }

    # remove existing installation
    if (Test-Path $installDir) {
        $doDeletion = $False
        if ("$env:BCAD_INSTALL_QUIET" -ne "") {
            $doDeletion = $True
        }
        else {
            $deleteExistingP = Read-Host "Existing installation found, delete it?  [Y/N]"
            if ($deleteExistingP.ToUpper() -eq "Y") {
                $doDeletion = $True
            }
        }

        if (-Not $doDeletion) {
            Write-Host "Exiting install"
            exit 0
        }

        $retryCountMax = 5
        For ($i = 0; $i -lt $retryCountMax; $i++) {
            try {
                Remove-Item $installDir -Recurse -Force
                break
            }
            catch {
                Start-Sleep -Seconds 2
            }
        }

        if (Test-Path $installDir) {
            Write-Host "Unable to remove existing installation"
            exit 1
        }
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
