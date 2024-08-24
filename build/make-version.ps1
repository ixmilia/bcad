#!/usr/bin/pwsh

[CmdletBinding(PositionalBinding = $false)]
param (
    [string]$suffix = "dev"
)

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

try {
    # get date of latest commit
    $date = git show --no-patch --format=%cs HEAD

    # count commits with that same date
    $lines = git log '--pretty=%cs' -n 99
    $count = 0
    foreach ($line in $lines) {
        if ($line -ne $date) {
            break
        }

        $count++
    }

    # format date as version
    $parts = $date -Split '-'
    $year = $parts[0] # e.g., 2024 from '2024-12-25'
    $month = $parts[1] # e.g., 12 from '2024-12-25'
    $day = $parts[2] # e.g., 25 from '2024-12-25'
    $version = "$suffix.$year-$month-$day.$count"
    Write-Output $version

}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}
