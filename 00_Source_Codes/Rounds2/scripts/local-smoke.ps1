param(
    [string]$ExecutablePath = "Builds\Client\Rounds2Client.exe",
    [string]$LogDirectory = "Logs",
    [int]$ServerWarmupSeconds = 10,
    [int]$ClientJoinSeconds = 20
)

$ErrorActionPreference = "Stop"

$resolvedExe = Resolve-Path $ExecutablePath -ErrorAction Stop
New-Item -ItemType Directory -Force -Path $LogDirectory | Out-Null

$serverLog = Join-Path $LogDirectory "local-smoke-server.log"
$client1Log = Join-Path $LogDirectory "local-smoke-client-1.log"
$client2Log = Join-Path $LogDirectory "local-smoke-client-2.log"
Remove-Item -Force -ErrorAction SilentlyContinue $serverLog, $client1Log, $client2Log

$processes = @()

function Start-Rounds2Process {
    param(
        [string[]]$Arguments
    )

    Start-Process -FilePath $resolvedExe.Path -ArgumentList $Arguments -PassThru -WindowStyle Hidden
}

function Assert-LogContains {
    param(
        [string]$Path,
        [string]$Pattern
    )

    if (-not (Select-String -Path $Path -Pattern $Pattern -Quiet)) {
        throw "Expected '$Pattern' in $Path."
    }
}

function Assert-LogDoesNotContain {
    param(
        [string]$Path,
        [string]$Pattern
    )

    if (Select-String -Path $Path -Pattern $Pattern -Quiet) {
        throw "Unexpected '$Pattern' in $Path."
    }
}

try {
    $processes += Start-Rounds2Process @("-batchmode", "-nographics", "-server", "-logFile", $serverLog)
    Start-Sleep -Seconds $ServerWarmupSeconds

    $processes += Start-Rounds2Process @("-batchmode", "-nographics", "-client", "-logFile", $client1Log)
    Start-Sleep -Seconds 5
    $processes += Start-Rounds2Process @("-batchmode", "-nographics", "-client", "-logFile", $client2Log)
    Start-Sleep -Seconds $ClientJoinSeconds

    foreach ($log in @($serverLog, $client1Log, $client2Log)) {
        Assert-LogDoesNotContain $log "InvalidOperationException"
        Assert-LogDoesNotContain $log "Player prefab is empty"
        Assert-LogDoesNotContain $log "spawned but it's recommended"
    }

    Assert-LogContains $serverLog "Rounds2 bootstrap launch mode: Server"
    Assert-LogContains $serverLog "Rounds2 loading online scene: Arena01"
    Assert-LogContains $serverLog "Rounds2 spawned player for connection 0"
    Assert-LogContains $serverLog "Rounds2 spawned player for connection 1"
    Assert-LogContains $client1Log "Rounds2 bootstrap launch mode: Client"
    Assert-LogContains $client2Log "Rounds2 bootstrap launch mode: Client"

    Write-Host "Rounds2 local smoke passed."
}
finally {
    foreach ($process in $processes) {
        if ($process -ne $null -and -not $process.HasExited) {
            Stop-Process -Id $process.Id -ErrorAction SilentlyContinue
        }
    }
}
