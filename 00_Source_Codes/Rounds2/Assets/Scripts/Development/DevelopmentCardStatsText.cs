using Rounds2.Cards;

namespace Rounds2.Development
{
    public static class DevelopmentCardStatsText
    {
        public static string Format(CombatCardStats stats, int cardCount)
        {
            ShotProfile shot = stats.ShotProfile;
            return string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "CARDS {0} | mag {1} reload {2:0.00}s | shot {3}x{4} cost {5} spread {6:0} | dmg x{7:0.00} speed x{8:0.00} bounce {9} | shield {10:0.00}s/{11:0.00}s",
                cardCount,
                stats.MagazineSize,
                stats.ReloadSeconds,
                shot.ProjectileCount,
                shot.BurstCount,
                shot.AmmoCost,
                shot.SpreadAngleDegrees,
                shot.DamageMultiplier,
                shot.SpeedMultiplier,
                shot.ProjectileBounces,
                stats.ShieldActiveSeconds,
                stats.ShieldCooldownSeconds);
        }
    }
}
