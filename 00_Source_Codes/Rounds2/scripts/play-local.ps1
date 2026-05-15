param(
    [string]$ExecutablePath = "Builds\Client\Rounds2Client.exe",
    [string]$LogDirectory = "Logs",
    [int]$ServerWarmupSeconds = 8
)

$ErrorActionPreference = "Stop"

$resolvedExe = Resolve-Path $ExecutablePath -ErrorAction Stop
New-Item -ItemType Directory -Force -Path $LogDirectory | Out-Null

$serverLog = Join-Path $LogDirectory "local-play-server.log"
$client1Log = Join-Path $LogDirectory "local-play-client-1.log"
$client2Log = Join-Path $LogDirectory "local-play-client-2.log"
$pidFile = Join-Path $LogDirectory "local-play-pids.txt"
Remove-Item -Force -ErrorAction SilentlyContinue $serverLog, $client1Log, $client2Log, $pidFile

$server = Start-Process -FilePath $resolvedExe.Path `
    -ArgumentList @("-batchmode", "-nographics", "-server", "-logFile", $serverLog) `
    -PassThru `
    -WindowStyle Hidden

Start-Sleep -Seconds $ServerWarmupSeconds

$clientArgs = @("-screen-fullscreen", "0", "-screen-width", "960", "-screen-height", "540", "-client")
$client1 = Start-Process -FilePath $resolvedExe.Path `
    -ArgumentList ($clientArgs + @("-logFile", $client1Log)) `
    -PassThru

Start-Sleep -Seconds 2

$client2 = Start-Process -FilePath $resolvedExe.Path `
    -ArgumentList ($clientArgs + @("-logFile", $client2Log)) `
    -PassThru

@($server.Id, $client1.Id, $client2.Id) | Set-Content -Path $pidFile

Write-Host "Rounds2 local play started."
Write-Host "Server PID: $($server.Id)"
Write-Host "Client 1 PID: $($client1.Id)"
Write-Host "Client 2 PID: $($client2.Id)"
Write-Host "Stop with: powershell -ExecutionPolicy Bypass -File scripts\stop-local.ps1"
