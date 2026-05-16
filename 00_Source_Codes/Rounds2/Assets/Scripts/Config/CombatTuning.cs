namespace Rounds2.Config
{
    public static class CombatTuning
    {
        public const int BaseHealth = 100;
        public const int BulletDamage = 25;
        public const float FireIntervalSeconds = 0.75f;
        public const float BulletSpeed = 12f;
        public const float BulletLeakSafetyLifetimeSeconds = 30f;
        public const float BulletKnockbackSpeed = BulletSpeed * 0.8f;
        public const uint BulletKnockbackDurationTicks = 6;
        public const float MoveSpeed = 6f;
        public const float MoveAcceleration = MoveSpeed / 0.1f;
        public const float MoveDeceleration = MoveSpeed / 0.12f;
        public const float MuzzleForwardOffset = 0.55f;
        public const float AimIndicatorLength = 0.95f;
        public const float AimIndicatorThickness = 0.08f;
        public const float BulletSpawnForwardOffset = MuzzleForwardOffset + AimIndicatorLength * 0.5f;
        public const float FireSpawnPredictionTolerance = 1.25f;
    }
}
