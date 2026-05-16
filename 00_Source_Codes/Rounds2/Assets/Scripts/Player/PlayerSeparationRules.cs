using UnityEngine;

namespace Rounds2.Player
{
    public static class PlayerSeparationRules
    {
        private const float Epsilon = 0.0001f;

        public static Vector2 FilterVelocity(
            Vector2 selfPosition,
            Vector2 otherPosition,
            Vector2 desiredVelocity,
            float minimumDistance,
            float deltaTime)
        {
            Vector2 delta = selfPosition - otherPosition;
            if (delta.sqrMagnitude <= Epsilon)
            {
                return desiredVelocity;
            }

            Vector2 nextDelta = selfPosition + desiredVelocity * deltaTime - otherPosition;
            bool alreadyTooClose = delta.sqrMagnitude < minimumDistance * minimumDistance;
            bool willBecomeTooClose = nextDelta.sqrMagnitude < minimumDistance * minimumDistance;

            if (!alreadyTooClose && !willBecomeTooClose)
            {
                return desiredVelocity;
            }

            return RemoveInwardVelocity(desiredVelocity, delta.normalized);
        }

        public static Vector2 RemoveInwardVelocity(Vector2 velocity, Vector2 awayDirection)
        {
            if (awayDirection.sqrMagnitude <= Epsilon)
            {
                return velocity;
            }

            Vector2 normalizedAway = awayDirection.normalized;
            float inwardSpeed = Vector2.Dot(velocity, normalizedAway);
            return inwardSpeed < 0f ? velocity - normalizedAway * inwardSpeed : velocity;
        }
    }
}
