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

- Server starts.
- Two clients connect to the local server.
- Both clients see both players.
- Players move with WASD and aim with the mouse.
- Left click fires a semi-auto bullet.
- Four hits defeat a player.
- The server logs `Set winner` after one player dies.

Run the automated connection smoke test after building the client:

```powershell
.\scripts\local-smoke.ps1
```

This starts one hidden client build as the temporary server and two hidden client builds as clients. It checks the logs for `Arena01` load and two player spawns.

## Build From Command Line

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.3.15f1\Editor\Unity.exe' -batchmode -quit -projectPath . -executeMethod Rounds2.Editor.Rounds2Build.BuildWindowsClient
& 'C:\Program Files\Unity\Hub\Editor\6000.3.15f1\Editor\Unity.exe' -batchmode -quit -projectPath . -executeMethod Rounds2.Editor.Rounds2Build.BuildWindowsServer
```

`BuildWindowsServer` fails until Windows Dedicated Server Build Support is installed for this Unity version.
