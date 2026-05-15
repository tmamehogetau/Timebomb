param(
    [string]$LogDirectory = "Logs"
)

$pidFile = Join-Path $LogDirectory "local-play-pids.txt"

if (-not (Test-Path $pidFile)) {
    Write-Host "No local play PID file found."
    exit 0
}

Get-Content $pidFile | ForEach-Object {
    $processId = 0
    if ([int]::TryParse($_, [ref]$processId)) {
        Stop-Process -Id $processId -ErrorAction SilentlyContinue
    }
}

Remove-Item -Force -ErrorAction SilentlyContinue $pidFile
Write-Host "Rounds2 local play stopped."
