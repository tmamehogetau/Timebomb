namespace Rounds2.Config
{
    public static class CombatTuning
    {
        public const int BaseHealth = 100;
        public const int BulletDamage = 25;
        public const float FireIntervalSeconds = 0.75f;
        public const int MagazineSize = 6;
        public const float ReloadSeconds = 1.5f;
        public const float ShieldActiveSeconds = 0.35f;
        public const float ShieldCooldownSeconds = 4f;
        public const float BulletSpeed = 12f;
        public const float BulletLeakSafetyLifetimeSeconds = 30f;
        public const float BulletKnockbackSpeed = BulletSpeed * 0.8f;
        public const uint BulletKnockbackDurationTicks = 6;
        public const float MoveSpeed = 6f;
        public const float MoveAcceleration = MoveSpeed / 0.1f;
        public const float MoveDeceleration = MoveSpeed / 0.12f;
        public const float MuzzleForwardOffset = 0.55f;
        public const float AimIndicatorLength = 0.75f;
        public const float AimIndicatorThickness = 0.08f;
        public const float BulletSpawnForwardOffset = MuzzleForwardOffset + AimIndicatorLength;
        public const float FireSpawnPredictionTolerance = 1.25f;
        public const float FireFeedbackRecoilDistance = 0.08f;
        public const float FireFeedbackRecoilSeconds = 0.08f;
        public const float MuzzleFlashSeconds = 0.05f;
        public const float MuzzleFlashScale = 0.14f;
        public const float BulletTrailSeconds = 0.08f;
        public const float BulletTrailStartWidth = 0.055f;
        public const float BulletTrailEndWidth = 0f;
        public const float BulletWallImpactLingerSeconds = 0.08f;
        public const float DamageFeedbackSeconds = 0.1f;
        public const float DamageFeedbackWhiteBlend = 0.45f;
        public const float ShieldBlockFeedbackSeconds = 0.12f;
        public const float ShieldBlockFeedbackAlphaBoost = 0.28f;
        public const float ShieldBlockFeedbackScaleBoost = 0.12f;
    }
}
