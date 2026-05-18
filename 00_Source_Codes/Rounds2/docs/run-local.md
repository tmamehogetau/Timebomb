# Run Local Rounds2

## Server

Build a Windows server target from Unity and run it with:

```powershell
Builds\Server\Rounds2Server.exe -server
```

This requires Unity's Windows Dedicated Server Build Support module. Until that module is installed, the client build can be used as a temporary local server:

```powershell
Builds\Client\Rounds2Client.exe -server
```

## Client

Run a Windows client build or the Unity Editor with:

```powershell
Builds\Client\Rounds2Client.exe -client
```

## Current Smoke Test

The MVP smoke target is:

- Server starts.
- Two clients connect to the local server.
- Combat starts on `Arena01`.
- Players can shoot, reload, shield, damage, and die.
- The server logs set and round winners.
- Loser-side draft occurs after round loss.
- Card rewards apply to later combat.
- A match reaches `Match winner`.
- Logs do not contain matched runtime errors.

Run the automated connection smoke test after building the client:

```powershell
.\scripts\local-smoke.ps1
```

This starts one hidden client build as the temporary server and two hidden client builds as clients. It checks the logs for `Arena01` load and two player spawns.

For a longer Bot-based MVP smoke run, start two Bot clients:

```powershell
.\scripts\play-local.ps1 -BotSecondClient $true
```

Then watch these logs:

```powershell
Select-String -Path Logs\local-play-server.log -Pattern 'Set winner|Round winner|Card draft|Card reward|Match winner'
Select-String -Path Logs\local-play-server.log,Logs\local-play-client-1.log,Logs\local-play-client-2.log -Pattern 'Exception|InvalidOperationException|Cannot complete action|error CS|\bError\b'
```

The accepted MVP smoke run on 2026-05-18 reached match end during a 10 minute Bot run with:

- 9 set winners
- 7 round winners
- 6 card drafts
- 6 card rewards
- 1 match winner
- 0 matched runtime errors

For manual playtesting with visible client windows:

```powershell
.\scripts\play-local.ps1
.\scripts\stop-local.ps1
```

`play-local.ps1` starts one hidden temporary server and two visible clients. Use `stop-local.ps1` to close the three launched processes.

## Build From Command Line

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.3.15f1\Editor\Unity.exe' -batchmode -quit -projectPath . -executeMethod Rounds2.Editor.Rounds2Build.BuildWindowsClient
& 'C:\Program Files\Unity\Hub\Editor\6000.3.15f1\Editor\Unity.exe' -batchmode -quit -projectPath . -executeMethod Rounds2.Editor.Rounds2Build.BuildWindowsServer
```

`BuildWindowsServer` fails until Windows Dedicated Server Build Support is installed for this Unity version.
