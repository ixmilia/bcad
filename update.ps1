Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

try {
    $architecture = if ($env:PROCESSOR_ARCHITECTURE -eq "AMD64") { "x64" } else { "arm64" }
    $packageName = "bcad-win-$architecture"
    $programsDir = Join-Path $env:LOCALAPPDATA "Programs"
    $installDir = Join-Path $programsDir $packageName

    # wait for process to exit
    $currentProcessId = "$env:BCAD_CURRENT_PROCESS_ID"
    if ($currentProcessId -ne "") {
        # wait for process to exit
        try {
            $currentProcess = Get-Process -Id $currentProcessId
            $waitTimeout = 10 * 1000 # 10s
            $hasExited = $currentProcess.WaitForExit($waitTimeout)
            if (-Not $hasExited) {
                # process didn't exit on time, just quit
                exit 1
            }
        }
        catch {
            # process not found, must have already exited
        }
    }

    # delete existing
    if (Test-Path $installDir) {
        Remove-Item $installDir -Recurse -Force
    }

    # install quietly
    $env:BCAD_INSTALL_QUIET = "1"
    Invoke-Expression (Invoke-WebRequest "https://pkgs.ixmilia.com/bcad/win/install.ps1").ToString()
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}
