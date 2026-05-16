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
