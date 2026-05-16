param(
    [string]$LogDirectory = "Logs",
    [string]$ExecutablePath = "Builds\Client\Rounds2Client.exe"
)

$pidFile = Join-Path $LogDirectory "local-play-pids.txt"
$processIds = [System.Collections.Generic.List[int]]::new()

if (Test-Path $pidFile) {
    Get-Content $pidFile | ForEach-Object {
        $processId = 0
        if ([int]::TryParse($_, [ref]$processId)) {
            $processIds.Add($processId)
        }
    }
}

$resolvedExe = Resolve-Path $ExecutablePath -ErrorAction SilentlyContinue
if ($resolvedExe -ne $null) {
    Get-Process Rounds2Client -ErrorAction SilentlyContinue | ForEach-Object {
        if ($_.Path -eq $resolvedExe.Path) {
            $processIds.Add($_.Id)
        }
    }
}

$uniqueProcessIds = $processIds | Select-Object -Unique
if ($uniqueProcessIds.Count -eq 0) {
    Write-Host "No local play processes found."
    Remove-Item -Force -ErrorAction SilentlyContinue $pidFile
    exit 0
}

foreach ($processId in $uniqueProcessIds) {
    Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
    Wait-Process -Id $processId -Timeout 5 -ErrorAction SilentlyContinue
}

Remove-Item -Force -ErrorAction SilentlyContinue $pidFile
Write-Host "Rounds2 local play stopped."
