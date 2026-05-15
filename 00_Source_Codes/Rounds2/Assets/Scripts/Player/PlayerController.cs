using FishNet.Object;
using Rounds2.Config;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rounds2.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerController : NetworkBehaviour
    {
        private Rigidbody2D body;
        private Vector2 moveInput;
        private Vector2 aimDirection = Vector2.right;

        public Vector2 AimDirection => aimDirection;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (!IsOwner)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;
            Vector2 requestedMove = ReadMoveInput(keyboard);
            Vector2 requestedAim = aimDirection;

            if (Camera.main != null && mouse != null)
            {
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
                Vector2 toMouse = mouseWorld - transform.position;
                if (toMouse.sqrMagnitude > 0.001f)
                {
                    requestedAim = toMouse.normalized;
                }
            }

            SubmitInputServerRpc(PlayerMotion.ClampMoveInput(requestedMove), requestedAim);
        }

        [ServerRpc]
        private void SubmitInputServerRpc(Vector2 move, Vector2 aim)
        {
            moveInput = PlayerMotion.ClampMoveInput(move);
            if (aim.sqrMagnitude > 0.001f)
            {
                aimDirection = aim.normalized;
            }
        }

        private void FixedUpdate()
        {
            if (!IsServer)
            {
                return;
            }

            body.linearVelocity = moveInput * CombatTuning.MoveSpeed;
            body.MoveRotation(PlayerMotion.AimAngleDegrees(aimDirection));
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

            return move;
        }
    }
}
