using Rounds2.Config;
using UnityEngine;

namespace Rounds2.Combat
{
    public static class WeaponSpawnPoint
    {
        public static Vector2 FromShooter(Vector2 shooterPosition, Vector2 aimDirection)
        {
            Vector2 direction = aimDirection.sqrMagnitude > 0.001f ? aimDirection.normalized : Vector2.right;
            return shooterPosition + direction * CombatTuning.BulletSpawnForwardOffset;
        }

        public static Vector2 ResolveRequestedSpawn(Vector2 serverSpawn, Vector2 requestedSpawn, Vector2 aimDirection)
        {
            float maxDistance = CombatTuning.FireSpawnPredictionTolerance;
            return Vector2.Distance(serverSpawn, requestedSpawn) <= maxDistance ? requestedSpawn : serverSpawn;
        }
    }
}
