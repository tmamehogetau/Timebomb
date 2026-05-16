using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using FishNet.Utility.Template;
using Rounds2.Config;
using Rounds2.Development;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rounds2.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PredictedPlayerMotor : TickNetworkBehaviour
    {
        private readonly PredictionRigidbody2D predictedBody = new();
        private readonly PlayerExternalForceQueue externalForces = new();
        private readonly SyncVar<Vector2> syncedAimDirection = new(Vector2.right);

        private Rigidbody2D body;
        private Transform aimIndicator;
        private Vector2 moveInput;
        private Vector2 movementVelocity;
        private Vector2 aimDirection = Vector2.right;
        private Vector2 displayAimDirection = Vector2.right;
        private Vector2 targetAimDirection = Vector2.right;
        private Vector2 lastSentAimDirection = Vector2.right;
        private float lastAimSyncTimeSeconds;

        private const float AimIndicatorFollowSpeed = 18f;
        private const float AimSyncIntervalSeconds = 1f / 30f;
        private const float AimSyncThresholdSquared = 0.000001f;

        public Vector2 AimDirection => aimDirection;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            aimIndicator = transform.Find("AimIndicator");
            predictedBody.Initialize(body);
            syncedAimDirection.UpdateSendRate(0f);
            syncedAimDirection.OnChange += OnSyncedAimDirectionChanged;
            ApplyAimIndicator();
        }

        private void Update()
        {
            if (!IsOwner)
            {
                UpdateRemoteAimIndicator();
            }
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
            SetAimDirection(input.Aim, snapDisplay: true);
            TrySyncAimDirection();
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

            Vector2 move = DevelopmentRuntimeOptions.BotEnabled ? moveInput : ReadMoveInput(Keyboard.current);
            return new PlayerReplicateData(move, aimDirection, fire: false);
        }

        [Replicate]
        private void Move(PlayerReplicateData data, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
        {
            if (ShouldApplyPredictedAim())
            {
                SetAimDirection(data.Aim.sqrMagnitude > 0.001f ? data.Aim : aimDirection, snapDisplay: IsOwner);
            }

            Vector2 targetMoveVelocity = data.Move * CombatTuning.MoveSpeed;
            movementVelocity = PlayerMotion.SmoothMoveVelocity(
                movementVelocity,
                targetMoveVelocity,
                NetworkTuning.FixedDeltaTime,
                CombatTuning.MoveAcceleration,
                CombatTuning.MoveDeceleration);
            Vector2 externalVelocity = externalForces.ConsumeTickVelocity();
            predictedBody.Velocity(movementVelocity + externalVelocity);
            predictedBody.Simulate();
        }

        public override void CreateReconcile()
        {
            PlayerReconcileData data = new(predictedBody, aimDirection, movementVelocity);
            Reconcile(data);
        }

        [Reconcile]
        private void Reconcile(PlayerReconcileData data, Channel channel = Channel.Unreliable)
        {
            if (ShouldApplyPredictedAim())
            {
                SetAimDirection(data.Aim.sqrMagnitude > 0.001f ? data.Aim : Vector2.right, snapDisplay: IsOwner);
            }

            movementVelocity = data.MoveVelocity;
            predictedBody.Reconcile(data.Body);
        }

        [Server]
        public void ApplyExternalForce(Vector2 direction, float speed, uint durationTicks)
        {
            EnqueueExternalForce(direction, speed, durationTicks);

            if (IsSpawned && Owner.IsValid)
            {
                ApplyExternalForceTargetRpc(Owner, direction, speed, durationTicks);
            }
        }

        [TargetRpc(ExcludeServer = true)]
        private void ApplyExternalForceTargetRpc(NetworkConnection connection, Vector2 direction, float speed, uint durationTicks)
        {
            EnqueueExternalForce(direction, speed, durationTicks);
        }

        private void EnqueueExternalForce(Vector2 direction, float speed, uint durationTicks)
        {
            externalForces.Enqueue(PlayerExternalForce.Create(direction, speed, durationTicks));
        }

        [Server]
        public void ResetRoundTransform(Vector3 position, Quaternion rotation)
        {
            if (IsSpawned)
            {
                ResetRoundTransformObserversRpc(position, rotation);
                return;
            }

            ApplyRoundReset(position, rotation);
        }

        [ObserversRpc(RunLocally = true)]
        private void ResetRoundTransformObserversRpc(Vector3 position, Quaternion rotation)
        {
            ApplyRoundReset(position, rotation);
        }

        private void ApplyRoundReset(Vector3 position, Quaternion rotation)
        {
            body ??= GetComponent<Rigidbody2D>();

            moveInput = Vector2.zero;
            movementVelocity = Vector2.zero;
            externalForces.Clear();
            Vector2 resetAim = rotation * Vector2.right;
            SetAimDirection(resetAim.sqrMagnitude > 0.001f ? resetAim : Vector2.right, snapDisplay: true);
            lastSentAimDirection = aimDirection;
            PlayerRoundReset.Apply(transform, body, position, rotation);
            predictedBody.ClearVelocities();
        }

        private void TrySyncAimDirection()
        {
            if (!IsOwner
                || !IsSpawned
                || Time.unscaledTime - lastAimSyncTimeSeconds < AimSyncIntervalSeconds
                || (aimDirection - lastSentAimDirection).sqrMagnitude < AimSyncThresholdSquared)
            {
                return;
            }

            lastSentAimDirection = aimDirection;
            lastAimSyncTimeSeconds = Time.unscaledTime;
            SyncAimDirectionServerRpc(aimDirection);
        }

        [ServerRpc]
        private void SyncAimDirectionServerRpc(Vector2 aim)
        {
            Vector2 normalizedAim = aim.sqrMagnitude > 0.001f ? aim.normalized : Vector2.right;
            SetAimDirection(normalizedAim, snapDisplay: true);
            syncedAimDirection.Value = normalizedAim;
        }

        private void OnSyncedAimDirectionChanged(Vector2 previous, Vector2 next, bool asServer)
        {
            if (asServer || IsOwner)
            {
                return;
            }

            SetAimDirection(next);
        }

        private bool ShouldApplyPredictedAim() => IsOwner || IsServerInitialized;

        private void SetAimDirection(Vector2 aim, bool snapDisplay = false)
        {
            aimDirection = aim.sqrMagnitude > 0.001f ? aim.normalized : Vector2.right;
            targetAimDirection = aimDirection;
            if (snapDisplay || !IsSpawned)
            {
                displayAimDirection = aimDirection;
            }

            ApplyAimIndicator();
        }

        private void UpdateRemoteAimIndicator()
        {
            displayAimDirection = Vector2.Lerp(displayAimDirection, targetAimDirection, Time.deltaTime * AimIndicatorFollowSpeed);
            if (displayAimDirection.sqrMagnitude <= 0.001f)
            {
                displayAimDirection = targetAimDirection;
            }
            else
            {
                displayAimDirection.Normalize();
            }

            ApplyAimIndicator();
        }

        private void ApplyAimIndicator()
        {
            if (aimIndicator == null)
            {
                return;
            }

            Vector3 localDirection = new(displayAimDirection.x, displayAimDirection.y, 0f);
            float distance = CombatTuning.MuzzleForwardOffset + CombatTuning.AimIndicatorLength * 0.5f;
            aimIndicator.localPosition = localDirection * distance;
            aimIndicator.localRotation = Quaternion.Euler(0f, 0f, PlayerMotion.AimAngleDegrees(displayAimDirection));
        }

        private static Vector2 ReadMoveInput(Keyboard keyboard)
        {
            if (keyboard == null)
            {
                return Vector2.zero;
            }

            Vector2 move = Vector2.zero;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                move.x -= 1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                move.x += 1f;
            }

            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                move.y -= 1f;
            }

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                move.y += 1f;
            }

            return PlayerMotion.ClampMoveInput(move);
        }
    }
}
