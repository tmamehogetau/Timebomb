using FishNet.Object;
using FishNet.Object.Prediction;
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

        private Rigidbody2D body;
        private Transform aimIndicator;
        private Vector2 moveInput;
        private Vector2 aimDirection = Vector2.right;

        public Vector2 AimDirection => aimDirection;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            aimIndicator = transform.Find("AimIndicator");
            predictedBody.Initialize(body);
            ApplyAimIndicator();
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
            SetAimDirection(input.Aim);
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
            SetAimDirection(data.Aim.sqrMagnitude > 0.001f ? data.Aim : aimDirection);
            predictedBody.Velocity(data.Move * CombatTuning.MoveSpeed);
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
            SetAimDirection(data.Aim.sqrMagnitude > 0.001f ? data.Aim : Vector2.right);
            predictedBody.Reconcile(data.Body);
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
            Vector2 resetAim = rotation * Vector2.right;
            SetAimDirection(resetAim.sqrMagnitude > 0.001f ? resetAim : Vector2.right);
            PlayerRoundReset.Apply(transform, body, position, rotation);
            predictedBody.ClearVelocities();
        }

        private void SetAimDirection(Vector2 aim)
        {
            aimDirection = aim.sqrMagnitude > 0.001f ? aim.normalized : Vector2.right;
            ApplyAimIndicator();
        }

        private void ApplyAimIndicator()
        {
            if (aimIndicator == null)
            {
                return;
            }

            Vector3 localDirection = new(aimDirection.x, aimDirection.y, 0f);
            float distance = CombatTuning.MuzzleForwardOffset + CombatTuning.AimIndicatorLength * 0.5f;
            aimIndicator.localPosition = localDirection * distance;
            aimIndicator.localRotation = Quaternion.Euler(0f, 0f, PlayerMotion.AimAngleDegrees(aimDirection));
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
