using Rounds2.Config;

namespace Rounds2.Cards
{
    public readonly struct CombatCardStats
    {
        public CombatCardStats(int magazineSize, float reloadSeconds, float shieldActiveSeconds, float shieldCooldownSeconds)
            : this(magazineSize, reloadSeconds, shieldActiveSeconds, shieldCooldownSeconds, ShotProfile.Base, projectileBounces: 0)
        {
        }

        public CombatCardStats(
            int magazineSize,
            float reloadSeconds,
            float shieldActiveSeconds,
            float shieldCooldownSeconds,
            ShotProfile shotProfile,
            int projectileBounces)
        {
            MagazineSize = magazineSize;
            ReloadSeconds = reloadSeconds;
            ShieldActiveSeconds = shieldActiveSeconds;
            ShieldCooldownSeconds = shieldCooldownSeconds;
            ShotProfile = shotProfile;
            ProjectileBounces = projectileBounces;
        }

        public int MagazineSize { get; }
        public float ReloadSeconds { get; }
        public float ShieldActiveSeconds { get; }
        public float ShieldCooldownSeconds { get; }
        public ShotProfile ShotProfile { get; }
        public int ProjectileBounces { get; }

        public static CombatCardStats Base => new(
            CombatTuning.MagazineSize,
            CombatTuning.ReloadSeconds,
            CombatTuning.ShieldActiveSeconds,
            CombatTuning.ShieldCooldownSeconds,
            ShotProfile.Base,
            projectileBounces: 0);
    }
}
