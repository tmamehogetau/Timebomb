using FishNet.Object;
using Rounds2.Config;
using UnityEngine;

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

            Vector2 requestedMove = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Vector2 requestedAim = aimDirection;

            if (Camera.main != null)
            {
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
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
    }
}
