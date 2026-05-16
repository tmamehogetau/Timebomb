using FishNet.Object;
using UnityEngine;

namespace Rounds2.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PredictedPlayerMotor))]
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

        [Server]
        public void ResetRoundTransform(Vector3 position, Quaternion rotation)
        {
            if (motor != null)
            {
                motor.ResetRoundTransform(position, rotation);
            }
        }
    }
}
