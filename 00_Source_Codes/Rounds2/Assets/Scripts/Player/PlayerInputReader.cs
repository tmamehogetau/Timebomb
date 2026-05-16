using Rounds2.Development;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rounds2.Player
{
    [DisallowMultipleComponent]
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
            if (motor == null || !motor.IsOwner || DevelopmentRuntimeOptions.BotEnabled)
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
