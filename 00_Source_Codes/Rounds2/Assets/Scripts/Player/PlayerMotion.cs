using UnityEngine;

namespace Rounds2.Player
{
    public static class PlayerMotion
    {
        public static Vector2 ClampMoveInput(Vector2 input)
        {
            return input.sqrMagnitude > 1f ? input.normalized : input;
        }

        public static float AimAngleDegrees(Vector2 aimDirection)
        {
            return Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        }
    }
}
