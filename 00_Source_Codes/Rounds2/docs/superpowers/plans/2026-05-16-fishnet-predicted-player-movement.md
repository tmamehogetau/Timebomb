# FishNet Predicted Player Movement Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the current hand-made player contact synchronization with FishNet prediction for player movement, so local control, player contact, and future knockback/card effects can share one authoritative simulation path.

**Architecture:** Player movement becomes tick-driven. The owner builds per-tick input, FishNet runs `[Replicate]` on owner/server/observers, and server reconcile state corrects drift. The first milestone only predicts player movement and contact; bullets remain server-spawned until movement is stable.

**Tech Stack:** Unity 6.3, FishNet, `TickNetworkBehaviour`, `PredictionRigidbody2D`, `IReplicateData`, `IReconcileData`, Unity EditMode tests, existing `scripts/local-smoke.ps1` and `scripts/play-local.ps1`.

---

## Decision

Do not continue extending `NetworkTransform + PlayerSeparationRules + owner-only visual distance` as the long-term solution.

Use FishNet prediction for player movement because Rounds2 needs player contact, knockback, shield timing, bullet hits, and card-modified movement to agree across clients. The current setup can be kept only as a temporary rollback point during migration, not as a foundation.

The migration must be incremental:

1. Move input and simple movement into prediction.
2. Verify movement without player contact.
3. Re-enable player contact under the predicted Rigidbody path.
4. Move knockback and external forces into the same predicted path.
5. Consider predicted/rollback-aware projectiles after the player movement layer is stable.

## Current Problems To Remove

- `PlayerController` owns too much: keyboard/mouse read, server RPC input, Rigidbody writes, contact filtering, round reset.
- `FixedUpdate` writes `body.linearVelocity` on both server and owner.
- Contact is implemented by filtering velocity against locally observed player positions, so owner and server may disagree near overlap.
- Dynamic `NetworkTransform.SetSendToOwner` caused visible stutter near enemies and must not return.
- `NetworkTransform` competes with any future predicted movement unless removed or disabled on predicted players.

## Target Shape

### Files To Create

- `Assets/Scripts/Player/PlayerMoveInput.cs`
  - Pure value type for movement and aim input.
  - Clamps movement and normalizes aim.

- `Assets/Scripts/Player/PlayerPredictionData.cs`
  - FishNet replicate/reconcile structs.
  - Holds move vector, aim vector, fire request placeholder, and `PredictionRigidbody2D`.

- `Assets/Scripts/Player/PredictedPlayerMotor.cs`
  - `TickNetworkBehaviour` that owns predicted Rigidbody simulation.
  - Builds owner input in tick, runs `[Replicate]`, runs `[Reconcile]`, and exposes `AimDirection`.

- `Assets/Scripts/Player/PlayerInputReader.cs`
  - Reads `Keyboard.current` and `Mouse.current`.
  - Returns `PlayerMoveInput`.
  - Is testable without networking where possible.

- `Assets/Tests/EditMode/PlayerMoveInputTests.cs`
  - Validates movement clamp and aim fallback.

- `Assets/Tests/EditMode/PredictedPlayerPrefabTests.cs`
  - Validates the player prefab uses prediction components and does not use `NetworkTransform` for movement.

- `Assets/Tests/EditMode/PredictedPlayerMotorSourceTests.cs`
  - Source-level guard for `[Replicate]`, `[Reconcile]`, `PredictionRigidbody2D`, `TimeManager_OnTick`, and `TimeManager_OnPostTick`.

### Files To Modify

- `Assets/Scripts/Player/PlayerController.cs`
  - Phase 1: keep as compatibility wrapper or remove after `WeaponController`, bot, reset, and HUD references are moved.
  - Final state: no direct Rigidbody movement, no ServerRpc input path, no contact filter.

- `Assets/Scripts/Combat/WeaponController.cs`
  - Replace `PlayerController` dependency with `PredictedPlayerMotor` or a small aim-provider interface.
  - Keep firing server-authoritative in the first migration.

- `Assets/Scripts/Player/PlayerBotController.cs`
  - Feed bot input through the same predicted input path as the owner.

- `Assets/Scripts/Match/SetManager.cs`
  - Reset predicted motor state, not only raw transform/body.

- `Assets/Scripts/Player/PlayerRoundReset.cs`
  - Keep as low-level helper, but call from predicted reset/reconcile path.

- `Assets/Editor/Rounds2ProjectSetup.cs`
  - Add `PredictionManager` if missing.
  - Set `TimeManager._physicsMode` to `PhysicsMode.TimeManager`.
  - Configure player prefab with `PredictedPlayerMotor`.
  - Remove or disable player `NetworkTransform` for the predicted player milestone.

- `Assets/Prefabs/Player.prefab`
  - Replace `PlayerController` movement role with `PredictedPlayerMotor`.
  - Remove player `NetworkTransform` once predicted movement owns transform/Rigidbody state.

- `Assets/Tests/EditMode/NetworkTransformPrefabTests.cs`
  - Stop requiring `NetworkTransform` on player.
  - Keep bullet `NetworkTransform` tests until bullet prediction is addressed.

- `Assets/Tests/EditMode/PlayerSeparationPrefabTests.cs`
  - Replace hand-made separation guard tests with prediction migration guard tests.

## Phase 0: Safety Baseline

### Task 0.1: Record Current Baseline

- [ ] Run EditMode tests.

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.15f1\Editor\Unity.exe" -batchmode -projectPath "C:\Users\kou-1\Dev_Workspace\00_Source_Codes\Rounds2" -runTests -testPlatform EditMode -testResults "C:\Users\kou-1\Dev_Workspace\00_Source_Codes\Rounds2\Logs\prediction-baseline-editmode.xml" -logFile "C:\Users\kou-1\Dev_Workspace\00_Source_Codes\Rounds2\Logs\prediction-baseline-editmode.log"
```

Expected: all current EditMode tests pass.

- [ ] Build current client.

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.15f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\kou-1\Dev_Workspace\00_Source_Codes\Rounds2" -executeMethod Rounds2.Editor.Rounds2Build.BuildWindowsClient -logFile "C:\Users\kou-1\Dev_Workspace\00_Source_Codes\Rounds2\Logs\prediction-baseline-build.log"
```

Expected log contains `Build Finished, Result: Success.`

- [ ] Run smoke.

```powershell
powershell -ExecutionPolicy Bypass -File scripts\local-smoke.ps1
```

Expected output contains `Rounds2 local smoke passed.`

- [ ] Start local play and note the current contact feel as the rollback baseline.

```powershell
powershell -ExecutionPolicy Bypass -File scripts\play-local.ps1
```

Expected: 2 clients launch; F2/F5 shortcuts still work.

## Phase 1: Introduce Prediction Data Without Changing Behavior

### Task 1.1: Add Pure Move Input Type

**Files:**
- Create: `Assets/Scripts/Player/PlayerMoveInput.cs`
- Create: `Assets/Tests/EditMode/PlayerMoveInputTests.cs`

- [ ] Write `PlayerMoveInputTests`.

```csharp
using NUnit.Framework;
using Rounds2.Player;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class PlayerMoveInputTests
    {
        [Test]
        public void CreateClampsMoveAndNormalizesAim()
        {
            PlayerMoveInput input = PlayerMoveInput.Create(new Vector2(2f, 2f), new Vector2(0f, 4f));

            Assert.LessOrEqual(input.Move.magnitude, 1.001f);
            Assert.AreEqual(Vector2.up.x, input.Aim.x, 0.001f);
            Assert.AreEqual(Vector2.up.y, input.Aim.y, 0.001f);
        }

        [Test]
        public void CreateKeepsFallbackAimWhenRequestedAimIsZero()
        {
            PlayerMoveInput input = PlayerMoveInput.Create(Vector2.zero, Vector2.zero, Vector2.left);

            Assert.AreEqual(Vector2.left.x, input.Aim.x, 0.001f);
            Assert.AreEqual(Vector2.left.y, input.Aim.y, 0.001f);
        }
    }
}
```

- [ ] Run the tests and verify they fail because `PlayerMoveInput` does not exist.

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.15f1\Editor\Unity.exe" -batchmode -projectPath "C:\Users\kou-1\Dev_Workspace\00_Source_Codes\Rounds2" -runTests -testPlatform EditMode -testResults "C:\Users\kou-1\Dev_Workspace\00_Source_Codes\Rounds2\Logs\player-move-input-red.xml" -logFile "C:\Users\kou-1\Dev_Workspace\00_Source_Codes\Rounds2\Logs\player-move-input-red.log"
```

Expected: compile failure mentioning `PlayerMoveInput`.

- [ ] Implement `PlayerMoveInput`.

```csharp
using UnityEngine;

namespace Rounds2.Player
{
    public readonly struct PlayerMoveInput
    {
        public PlayerMoveInput(Vector2 move, Vector2 aim)
        {
            Move = move;
            Aim = aim;
        }

        public Vector2 Move { get; }
        public Vector2 Aim { get; }

        public static PlayerMoveInput Create(Vector2 requestedMove, Vector2 requestedAim)
        {
            return Create(requestedMove, requestedAim, Vector2.right);
        }

        public static PlayerMoveInput Create(Vector2 requestedMove, Vector2 requestedAim, Vector2 fallbackAim)
        {
            Vector2 move = PlayerMotion.ClampMoveInput(requestedMove);
            Vector2 aim = requestedAim.sqrMagnitude > 0.001f
                ? requestedAim.normalized
                : (fallbackAim.sqrMagnitude > 0.001f ? fallbackAim.normalized : Vector2.right);

            return new PlayerMoveInput(move, aim);
        }
    }
}
```

- [ ] Run targeted EditMode tests and verify pass.

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.15f1\Editor\Unity.exe" -batchmode -projectPath "C:\Users\kou-1\Dev_Workspace\00_Source_Codes\Rounds2" -runTests -testPlatform EditMode -testResults "C:\Users\kou-1\Dev_Workspace\00_Source_Codes\Rounds2\Logs\player-move-input-green.xml" -logFile "C:\Users\kou-1\Dev_Workspace\00_Source_Codes\Rounds2\Logs\player-move-input-green.log"
```

Expected: `PlayerMoveInputTests` pass.

### Task 1.2: Add Prediction Replicate/Reconcile Types

**Files:**
- Create: `Assets/Scripts/Player/PlayerPredictionData.cs`
- Create or modify: `Assets/Tests/EditMode/PredictedPlayerMotorSourceTests.cs`

- [ ] Write source test.

```csharp
using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class PredictedPlayerMotorSourceTests
    {
        [Test]
        public void PredictionDataUsesFishNetReplicateAndReconcileInterfaces()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Player", "PlayerPredictionData.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("IReplicateData", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("IReconcileData", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("PredictionRigidbody2D", StringComparison.Ordinal));
        }
    }
}
```

- [ ] Run once and verify fail because file does not exist.

- [ ] Implement prediction data.

```csharp
using FishNet.Object.Prediction;
using UnityEngine;

namespace Rounds2.Player
{
    public struct PlayerReplicateData : IReplicateData
    {
        public Vector2 Move;
        public Vector2 Aim;
        public bool Fire;
        private uint tick;

        public PlayerReplicateData(Vector2 move, Vector2 aim, bool fire)
        {
            Move = move;
            Aim = aim;
            Fire = fire;
            tick = 0;
        }

        public void Dispose() { }
        public uint GetTick() => tick;
        public void SetTick(uint value) => tick = value;
    }

    public struct PlayerReconcileData : IReconcileData
    {
        public PredictionRigidbody2D Body;
        public Vector2 Aim;
        private uint tick;

        public PlayerReconcileData(PredictionRigidbody2D body, Vector2 aim)
        {
            Body = body;
            Aim = aim;
            tick = 0;
        }

        public void Dispose() { }
        public uint GetTick() => tick;
        public void SetTick(uint value) => tick = value;
    }
}
```

- [ ] Run targeted tests and verify pass.

## Phase 2: Add Predicted Motor With Contact Disabled

### Task 2.1: Implement `PredictedPlayerMotor`

**Files:**
- Create: `Assets/Scripts/Player/PredictedPlayerMotor.cs`
- Modify later: `Assets/Prefabs/Player.prefab`

- [ ] Add source test for motor shape.

```csharp
[Test]
public void PredictedMotorRunsReplicateAndReconcileOnFishNetTicks()
{
    string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Player", "PredictedPlayerMotor.cs");
    string source = File.ReadAllText(sourcePath);

    Assert.IsTrue(source.Contains("TickNetworkBehaviour", StringComparison.Ordinal));
    Assert.IsTrue(source.Contains("SetTickCallbacks", StringComparison.Ordinal));
    Assert.IsTrue(source.Contains("TimeManager_OnTick", StringComparison.Ordinal));
    Assert.IsTrue(source.Contains("TimeManager_OnPostTick", StringComparison.Ordinal));
    Assert.IsTrue(source.Contains("[Replicate]", StringComparison.Ordinal));
    Assert.IsTrue(source.Contains("[Reconcile]", StringComparison.Ordinal));
    Assert.IsTrue(source.Contains("PredictionRigidbody2D", StringComparison.Ordinal));
}
```

- [ ] Implement the first motor without player-player contact.

```csharp
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using FishNet.Utility.Template;
using Rounds2.Config;
using UnityEngine;

namespace Rounds2.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PredictedPlayerMotor : TickNetworkBehaviour
    {
        private readonly PredictionRigidbody2D predictedBody = new();
        private Rigidbody2D body;
        private Vector2 moveInput;
        private Vector2 aimDirection = Vector2.right;

        public Vector2 AimDirection => aimDirection;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            predictedBody.Initialize(body);
        }

        public override void OnStartNetwork()
        {
            SetTickCallbacks(TickCallback.Tick | TickCallback.PostTick);
        }

        public void SubmitOwnerInput(Vector2 move, Vector2 aim)
        {
            if (!IsOwner)
            {
                return;
            }

            PlayerMoveInput input = PlayerMoveInput.Create(move, aim, aimDirection);
            moveInput = input.Move;
            aimDirection = input.Aim;
        }

        protected override void TimeManager_OnTick()
        {
            Move(BuildReplicateData());
        }

        protected override void TimeManager_OnPostTick()
        {
            CreateReconcile();
        }

        private PlayerReplicateData BuildReplicateData()
        {
            if (!IsOwner)
            {
                return default;
            }

            return new PlayerReplicateData(moveInput, aimDirection, fire: false);
        }

        [Replicate]
        private void Move(PlayerReplicateData data, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
        {
            Vector2 aim = data.Aim.sqrMagnitude > 0.001f ? data.Aim.normalized : aimDirection;
            aimDirection = aim;

            Vector2 nextPosition = body.position + data.Move * CombatTuning.MoveSpeed * (float)TimeManager.TickDelta;
            predictedBody.MovePosition(nextPosition);
            predictedBody.MoveRotation(Quaternion.Euler(0f, 0f, PlayerMotion.AimAngleDegrees(aimDirection)));
            predictedBody.Simulate();
        }

        public override void CreateReconcile()
        {
            PlayerReconcileData data = new(predictedBody, aimDirection);
            Reconcile(data);
        }

        [Reconcile]
        private void Reconcile(PlayerReconcileData data, Channel channel = Channel.Unreliable)
        {
            aimDirection = data.Aim.sqrMagnitude > 0.001f ? data.Aim.normalized : Vector2.right;
            predictedBody.Reconcile(data.Body);
        }
    }
}
```

- [ ] Run tests and fix compile errors. Keep this step focused on compilation and no-contact movement.

### Task 2.2: Switch Input Reader To Feed Predicted Motor

**Files:**
- Create: `Assets/Scripts/Player/PlayerInputReader.cs`
- Modify: `Assets/Scripts/Player/PlayerController.cs` or replace its input role

- [ ] Create input reader.

```csharp
using Rounds2.Development;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rounds2.Player
{
    [RequireComponent(typeof(PredictedPlayerMotor))]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        private PredictedPlayerMotor motor;

        private void Awake()
        {
            motor = GetComponent<PredictedPlayerMotor>();
        }

        private void Update()
        {
            if (!motor.IsOwner || DevelopmentRuntimeOptions.BotEnabled)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;
            Vector2 requestedMove = ReadMoveInput(keyboard);
            Vector2 requestedAim = motor.AimDirection;

            if (Camera.main != null && mouse != null)
            {
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
                Vector2 toMouse = mouseWorld - transform.position;
                if (toMouse.sqrMagnitude > 0.001f)
                {
                    requestedAim = toMouse.normalized;
                }
            }

            motor.SubmitOwnerInput(requestedMove, requestedAim);
        }

        private static Vector2 ReadMoveInput(Keyboard keyboard)
        {
            if (keyboard == null)
            {
                return Vector2.zero;
            }

            Vector2 value = Vector2.zero;
            if (keyboard.wKey.isPressed) value.y += 1f;
            if (keyboard.sKey.isPressed) value.y -= 1f;
            if (keyboard.dKey.isPressed) value.x += 1f;
            if (keyboard.aKey.isPressed) value.x -= 1f;
            return PlayerMotion.ClampMoveInput(value);
        }
    }
}
```

- [ ] Remove input reading and direct `FixedUpdate` movement from `PlayerController`, or keep `PlayerController` disabled as a compatibility shell until all references are moved.

Expected final `PlayerController` during this phase:

```csharp
namespace Rounds2.Player
{
    public sealed class PlayerController : NetworkBehaviour
    {
        private PredictedPlayerMotor motor;

        public Vector2 AimDirection => motor != null ? motor.AimDirection : Vector2.right;

        private void Awake()
        {
            motor = GetComponent<PredictedPlayerMotor>();
        }

        public void SubmitOwnerInput(Vector2 move, Vector2 aim)
        {
            motor?.SubmitOwnerInput(move, aim);
        }
    }
}
```

## Phase 3: Prefab And Network Manager Migration

### Task 3.1: Configure Prediction Manager And TimeManager

**Files:**
- Modify: `Assets/Editor/Rounds2ProjectSetup.cs`
- Modify generated scene/prefab by running the editor setup if this project uses setup regeneration

- [ ] Add setup tests.

```csharp
[Test]
public void NetworkManagerUsesTimeManagerPhysicsForPrediction()
{
    string sourcePath = Path.Combine(Application.dataPath, "Editor", "Rounds2ProjectSetup.cs");
    string source = File.ReadAllText(sourcePath);

    Assert.IsTrue(source.Contains("PredictionManager", StringComparison.Ordinal));
    Assert.IsTrue(source.Contains("_physicsMode", StringComparison.Ordinal));
    Assert.IsTrue(source.Contains("PhysicsMode.TimeManager", StringComparison.Ordinal));
}
```

- [ ] Configure `NetworkManager`.

Implementation intent:

```csharp
using FishNet.Managing.Predicting;
using FishNet.Managing.Timing;

private static void ConfigureNetworkManagerTiming(NetworkManager networkManager)
{
    TimeManager timeManager = networkManager.GetComponent<TimeManager>();
    if (timeManager != null)
    {
        SerializedObject serializedTimeManager = new(timeManager);
        serializedTimeManager.FindProperty("_tickRate").intValue = NetworkTuning.TickRate;
        serializedTimeManager.FindProperty("_physicsMode").enumValueIndex = (int)PhysicsMode.TimeManager;
        serializedTimeManager.ApplyModifiedPropertiesWithoutUndo();
    }

    if (networkManager.GetComponent<PredictionManager>() == null)
    {
        networkManager.gameObject.AddComponent<PredictionManager>();
    }
}
```

Use the actual FishNet namespace in the installed package if it differs. Verify by compiling.

### Task 3.2: Convert Player Prefab

**Files:**
- Modify: `Assets/Prefabs/Player.prefab`
- Modify: `Assets/Editor/Rounds2ProjectSetup.cs`
- Modify: `Assets/Tests/EditMode/NetworkTransformPrefabTests.cs`
- Create/modify: `Assets/Tests/EditMode/PredictedPlayerPrefabTests.cs`

- [ ] Add prefab tests.

```csharp
using FishNet.Component.Transforming;
using NUnit.Framework;
using Rounds2.Player;
using UnityEditor;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class PredictedPlayerPrefabTests
    {
        [Test]
        public void PlayerPrefabUsesPredictedMotor()
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");

            Assert.IsNotNull(playerPrefab.GetComponent<PredictedPlayerMotor>());
            Assert.IsNotNull(playerPrefab.GetComponent<PlayerInputReader>());
        }

        [Test]
        public void PlayerPrefabDoesNotUseNetworkTransformForMovement()
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");

            Assert.IsNull(playerPrefab.GetComponent<NetworkTransform>());
        }
    }
}
```

- [ ] Update prefab generation.

Implementation intent in `CreatePlayerPrefab()`:

```csharp
ConfigureLowLatencyNetworkObject(playerObject.AddComponent<NetworkObject>());
Rigidbody2D body = playerObject.AddComponent<Rigidbody2D>();
body.gravityScale = 0f;
body.freezeRotation = true;
body.interpolation = RigidbodyInterpolation2D.None;
body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

playerObject.AddComponent<HealthVisuals>();
playerObject.AddComponent<Health>();
playerObject.AddComponent<PredictedPlayerMotor>();
playerObject.AddComponent<PlayerInputReader>();
playerObject.AddComponent<PlayerBotController>();
playerObject.AddComponent<PlayerDevelopmentShortcuts>();
playerObject.AddComponent<PlayerContactCollision>();
playerObject.AddComponent<PlayerOwnerVisuals>();
playerObject.AddComponent<WeaponController>();
```

Do not add `NetworkTransform` to player in the predicted movement milestone.

- [ ] Update existing prefab manually or regenerate it through the existing editor setup path.

- [ ] Run EditMode tests and resolve compile or prefab failures.

## Phase 4: Move Weapon And Bot Dependencies

### Task 4.1: Weapon Uses Predicted Aim

**Files:**
- Modify: `Assets/Scripts/Combat/WeaponController.cs`
- Modify: `Assets/Tests/EditMode/WeaponControllerLaunchOrderTests.cs`

- [ ] Change dependency from `PlayerController` to `PredictedPlayerMotor`.

Implementation intent:

```csharp
private PredictedPlayerMotor motor;

private void Awake()
{
    motor = GetComponent<PredictedPlayerMotor>();
    health = GetComponent<Health>();
}

private void Update()
{
    Mouse mouse = Mouse.current;
    if (!IsOwner || DevelopmentRuntimeOptions.BotEnabled || mouse == null || !mouse.leftButton.wasPressedThisFrame)
    {
        return;
    }

    Vector2 aim = motor != null ? motor.AimDirection : Vector2.right;
    TryFire(aim);
}
```

In `FireServerRpc`, use `motor.AimDirection` as fallback.

### Task 4.2: Bot Uses Same Input Path

**Files:**
- Modify: `Assets/Scripts/Player/PlayerBotController.cs`
- Modify: `Assets/Tests/EditMode/PlayerBotInputTests.cs` if needed

- [ ] Replace `PlayerController` with `PredictedPlayerMotor`.

Expected behavior:

- Bot still only runs when `DevelopmentRuntimeOptions.BotEnabled` is true.
- Bot calls `motor.SubmitOwnerInput(move, aim)`.
- Bot fire still calls `weapon.TryFire(aim)`.

## Phase 5: Round Reset Reconcile

### Task 5.1: Reset Predicted State Cleanly

**Files:**
- Modify: `Assets/Scripts/Player/PredictedPlayerMotor.cs`
- Modify: `Assets/Scripts/Match/SetManager.cs`
- Modify: `Assets/Tests/EditMode/PlayerRoundResetTests.cs`

- [ ] Add method on motor.

```csharp
[Server]
public void ResetRoundTransform(Vector3 position, Quaternion rotation)
{
    ResetRoundTransformObserversRpc(position, rotation);
}

[ObserversRpc(RunLocally = true)]
private void ResetRoundTransformObserversRpc(Vector3 position, Quaternion rotation)
{
    ApplyRoundReset(position, rotation);
}

private void ApplyRoundReset(Vector3 position, Quaternion rotation)
{
    moveInput = Vector2.zero;
    aimDirection = rotation * Vector2.right;
    PlayerRoundReset.Apply(transform, body, position, rotation);
    predictedBody.ClearVelocities();
}
```

- [ ] Update `SetManager` to call `PredictedPlayerMotor.ResetRoundTransform`.

Expected: both clients see the same respawn position after a kill and the next countdown.

## Phase 6: Reintroduce Player Contact Under Prediction

### Task 6.1: Remove Hand-Made Position Correction Permanently

**Files:**
- Modify: `Assets/Scripts/Player/PlayerContactCollision.cs`
- Modify: `Assets/Scripts/Player/PlayerSeparationRules.cs`
- Modify: `Assets/Tests/EditMode/PlayerSeparationPrefabTests.cs`

- [ ] Keep only collision-ignore behavior if needed for bullets/owner handling.
- [ ] Do not use `SetSendToOwner`.
- [ ] Do not use owner-only contact distances.
- [ ] Do not use position correction RPCs.

### Task 6.2: Test Physical Contact With Prediction

**Manual verification:**

1. Run local play.
2. Disable bot with `F2`.
3. Move both players into each other.
4. Verify:
   - owner movement starts without the one-time hitch;
   - approaching the other player does not create constant stutter;
   - pushing/contact does not launch either player out of bounds;
   - both windows converge to the same result after contact ends.

If contact still jitters:

- Prefer tuning Rigidbody2D material/friction/mass/drag and predicted movement mode.
- Do not re-add dynamic `SetSendToOwner`.
- Do not re-add per-client visual separation distances.

## Phase 7: External Forces And Future Cards

### Task 7.1: Add Predicted External Force Queue

**Files:**
- Modify: `Assets/Scripts/Player/PredictedPlayerMotor.cs`
- Create: `Assets/Scripts/Player/PlayerExternalForce.cs`
- Create: `Assets/Tests/EditMode/PlayerExternalForceTests.cs`

Purpose:

- Knockback from bullets.
- Future card effects: recoil, shield bash, slow, acceleration burst.

Implementation principle:

- Server-authoritative events enqueue a force with the tick where it happened.
- Replicate applies the same force during prediction/replay.
- Reconcile includes enough state to avoid double-applying forces.

Do not implement this until base movement/contact is accepted by manual testing.

### Task 7.2: Bullet Hit Timing Review

**Files:**
- `Assets/Scripts/Combat/Bullet.cs`
- `Assets/Scripts/Combat/WeaponController.cs`

Keep bullets server-authoritative for now. Only revisit predicted projectiles or rollback hitscan after:

- movement prediction is stable;
- player contact is stable;
- knockback is implemented through predicted force application.

## Verification Matrix

Run after each phase:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.15f1\Editor\Unity.exe" -batchmode -projectPath "C:\Users\kou-1\Dev_Workspace\00_Source_Codes\Rounds2" -runTests -testPlatform EditMode -testResults "C:\Users\kou-1\Dev_Workspace\00_Source_Codes\Rounds2\Logs\prediction-editmode.xml" -logFile "C:\Users\kou-1\Dev_Workspace\00_Source_Codes\Rounds2\Logs\prediction-editmode.log"
```

Run before asking for manual playtest:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.15f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\kou-1\Dev_Workspace\00_Source_Codes\Rounds2" -executeMethod Rounds2.Editor.Rounds2Build.BuildWindowsClient -logFile "C:\Users\kou-1\Dev_Workspace\00_Source_Codes\Rounds2\Logs\prediction-build.log"
powershell -ExecutionPolicy Bypass -File scripts\local-smoke.ps1
powershell -ExecutionPolicy Bypass -File scripts\play-local.ps1
```

Check runtime logs:

```powershell
Start-Sleep -Seconds 8
Select-String -Path Logs\local-play-client-1.log,Logs\local-play-client-2.log,Logs\local-play-server.log -Pattern "InvalidOperationException|Cannot complete action|Exception|error CS"
```

Expected: no matches.

## Manual Acceptance Criteria

Movement milestone is accepted only if:

- Local movement starts immediately without the old start hitch.
- Remote player movement is slightly delayed but smooth.
- Approaching another player does not produce constant stutter.
- Contact does not produce large divergence between windows.
- Round reset still places both players consistently.
- Bot toggle `F2` and reset `F5` still work.
- Bullets still spawn from the aim line and kill in 4 hits.

## Rollback Rule

Keep each phase small enough to revert independently. If Phase 2 compiles but feels worse than the current baseline, revert the prefab switch while keeping pure data/input types. If Phase 6 contact fails, keep predicted movement without contact and investigate Rigidbody2D/prediction collision settings before adding any hand-made correction.

## References

- FishNet installed package demo: `Library/PackageCache/com.firstgeargames.fishnet@0728292d8339/Demos/Prediction/Rigidbody/Scripts/RigidbodyPrediction.cs`
- FishNet installed source: `Library/PackageCache/com.firstgeargames.fishnet@0728292d8339/Runtime/Object/Prediction/PredictionRigidbody2D.cs`
- FishNet docs: `PredictionRigidbody`, client-side prediction, `NetworkTickSmoother`
- Unity docs: Rigidbody interpolation and physics simulation timing

