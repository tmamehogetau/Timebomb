using UnityEngine;

namespace Rounds2.Player
{
    public readonly struct PlayerBotCommand
    {
        public PlayerBotCommand(Vector2 move, Vector2 aim, bool shouldFire)
        {
            Move = move;
            Aim = aim;
            ShouldFire = shouldFire;
        }

        public Vector2 Move { get; }
        public Vector2 Aim { get; }
        public bool ShouldFire { get; }
    }

    public static class PlayerBotInput
    {
        private const float PreferredDistance = 3.2f;
        private const float DistanceSlack = 0.45f;
        private const float AimSwayDegrees = 18f;

        public static PlayerBotCommand Decide(
            Vector2 selfPosition,
            Vector2 targetPosition,
            float timeSeconds,
            float nextAllowedFireTimeSeconds)
        {
            Vector2 toTarget = targetPosition - selfPosition;
            if (toTarget.sqrMagnitude <= 0.001f)
            {
                return new PlayerBotCommand(Vector2.zero, Vector2.right, shouldFire: false);
            }

            Vector2 directAim = toTarget.normalized;
            Vector2 aim = Quaternion.Euler(0f, 0f, Mathf.Sin(timeSeconds * 1.3f + 0.8f) * AimSwayDegrees) * directAim;
            float distance = toTarget.magnitude;
            Vector2 move = Vector2.Perpendicular(directAim) * Mathf.Sign(Mathf.Sin(timeSeconds * 1.7f));

            if (distance > PreferredDistance + DistanceSlack)
            {
                move += directAim;
            }
            else if (distance < PreferredDistance - DistanceSlack)
            {
                move -= directAim;
            }

            return new PlayerBotCommand(move.normalized, aim.normalized, timeSeconds >= nextAllowedFireTimeSeconds);
        }
    }
}
