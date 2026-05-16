using UnityEngine;

namespace Rounds2.Player
{
    public static class PlayerMotion
    {
        public static Vector2 ClampMoveInput(Vector2 input)
        {
            return input.sqrMagnitude > 1f ? input.normalized : input;
        }

        public static Vector2 SmoothMoveVelocity(
            Vector2 currentVelocity,
            Vector2 targetVelocity,
            float deltaTime,
            float acceleration,
            float deceleration)
        {
            if (deltaTime <= 0f)
            {
                return currentVelocity;
            }

            float rate = targetVelocity.sqrMagnitude > 0.001f ? acceleration : deceleration;
            return Vector2.MoveTowards(currentVelocity, targetVelocity, rate * deltaTime);
        }

        public static float AimAngleDegrees(Vector2 aimDirection)
        {
            return Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        }
    }
}
