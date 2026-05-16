param(
    [string]$ScriptPath = "scripts\stop-local.ps1"
)

$ErrorActionPreference = "Stop"

$source = Get-Content -Raw -Path $ScriptPath

if ($source -notmatch 'Stop-Process\s+-Id\s+\$processId\s+-Force') {
    throw "stop-local.ps1 must force-stop launched processes."
}

if ($source -notmatch 'Wait-Process\s+-Id\s+\$processId') {
    throw "stop-local.ps1 must wait until launched processes exit."
}

if ($source -notmatch "Get-Process Rounds2Client") {
    throw "stop-local.ps1 must stop workspace Rounds2Client processes even when the PID file is missing."
}

Write-Host "stop-local script test passed."
