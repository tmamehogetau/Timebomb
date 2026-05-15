# Run Local Rounds2

## Server

Build a Windows server target from Unity and run it with:

```powershell
Builds\Server\Rounds2Server.exe -server
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
