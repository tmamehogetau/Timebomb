using System;
using UnityEngine;

namespace Rounds2.Cards
{
    public readonly struct ShotProfile
    {
        public const int MaxProjectileCount = 30;
        public const int MaxBurstCount = 15;
        public const int MaxAmmoCost = MaxProjectileCount * MaxBurstCount;
        public const float MaxSpreadAngleDegrees = 90f;
        public const float MinDamageMultiplier = 0.18f;
        public const float MaxDamageMultiplier = 3f;
        public const float BurstIntervalSeconds = 0.08f;

        public ShotProfile(
            int projectileCount,
            int burstCount,
            float spreadAngleDegrees,
            int ammoCost,
            float damageMultiplier,
            float speedMultiplier,
            int projectileBounces)
        {
            ProjectileCount = Mathf.Clamp(projectileCount, 1, MaxProjectileCount);
            BurstCount = Mathf.Clamp(burstCount, 1, MaxBurstCount);
            SpreadAngleDegrees = Mathf.Clamp(spreadAngleDegrees, 0f, MaxSpreadAngleDegrees);
            AmmoCost = Mathf.Clamp(ammoCost, 1, MaxAmmoCost);
            DamageMultiplier = Mathf.Clamp(damageMultiplier, MinDamageMultiplier, MaxDamageMultiplier);
            SpeedMultiplier = Math.Max(0.1f, speedMultiplier);
            ProjectileBounces = Mathf.Max(0, projectileBounces);
        }

        public int ProjectileCount { get; }
        public int BurstCount { get; }
        public float SpreadAngleDegrees { get; }
        public int AmmoCost { get; }
        public float DamageMultiplier { get; }
        public float SpeedMultiplier { get; }
        public int ProjectileBounces { get; }

        public static ShotProfile Base => new(
            projectileCount: 1,
            burstCount: 1,
            spreadAngleDegrees: 0f,
            ammoCost: 1,
            damageMultiplier: 1f,
            speedMultiplier: 1f,
            projectileBounces: 0);

        public static ShotProfile Create(int projectileCount, int burstCount, float spreadAngleDegrees)
        {
            return Create(projectileCount, burstCount, spreadAngleDegrees, speedMultiplier: 1f, damageMultiplier: 1f, projectileBounces: 0);
        }

        public static ShotProfile Create(
            int projectileCount,
            int burstCount,
            float spreadAngleDegrees,
            float speedMultiplier,
            float damageMultiplier,
            int projectileBounces)
        {
            int clampedProjectiles = Mathf.Clamp(projectileCount, 1, MaxProjectileCount);
            int clampedBursts = Mathf.Clamp(burstCount, 1, MaxBurstCount);
            int totalProjectiles = clampedProjectiles * clampedBursts;
            float multiProjectileDamageMultiplier = Mathf.Max(MinDamageMultiplier, 1f / Mathf.Sqrt(totalProjectiles));

            return new ShotProfile(
                clampedProjectiles,
                clampedBursts,
                spreadAngleDegrees,
                ammoCost: clampedProjectiles * clampedBursts,
                damageMultiplier: multiProjectileDamageMultiplier * damageMultiplier,
                speedMultiplier,
                projectileBounces);
        }
    }
}
