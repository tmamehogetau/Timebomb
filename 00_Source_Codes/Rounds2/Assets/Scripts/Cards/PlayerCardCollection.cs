using System.Collections.Generic;
using UnityEngine;

namespace Rounds2.Cards
{
    public sealed class PlayerCardCollection
    {
        private readonly List<CardId> cards = new();

        public int Count => cards.Count;
        public IReadOnlyList<CardId> Cards => cards;

        public void Grant(CardId card)
        {
            cards.Add(card);
        }

        public void Clear()
        {
            cards.Clear();
        }

        public CombatCardStats BuildStats()
        {
            int magazineSize = CombatCardStats.Base.MagazineSize;
            float reloadSeconds = CombatCardStats.Base.ReloadSeconds;
            float shieldActiveSeconds = CombatCardStats.Base.ShieldActiveSeconds;
            float shieldCooldownSeconds = CombatCardStats.Base.ShieldCooldownSeconds;
            int projectileCount = CombatCardStats.Base.ShotProfile.ProjectileCount;
            int burstCount = CombatCardStats.Base.ShotProfile.BurstCount;
            float spreadAngleDegrees = CombatCardStats.Base.ShotProfile.SpreadAngleDegrees;
            float projectileSpeedMultiplier = CombatCardStats.Base.ShotProfile.SpeedMultiplier;
            float projectileDamageMultiplier = 1f;
            int projectileBounces = CombatCardStats.Base.ProjectileBounces;
            HashSet<CardId> appliedNonCumulativeCards = new();

            foreach (CardId card in cards)
            {
                CardDefinition definition = CardCatalog.Get(card);
                if (definition.Stacking == CardStacking.NonCumulative && !appliedNonCumulativeCards.Add(card))
                {
                    continue;
                }

                reloadSeconds *= definition.ReloadMultiplier;
                shieldActiveSeconds += definition.ShieldActiveSecondsDelta;
                shieldCooldownSeconds *= definition.ShieldCooldownMultiplier;
                burstCount += definition.BurstCountDelta;
                projectileCount += definition.ProjectileCountDelta;
                spreadAngleDegrees += definition.SpreadAngleDegreesDelta;
                projectileSpeedMultiplier *= definition.ProjectileSpeedMultiplier;
                projectileDamageMultiplier *= definition.ProjectileDamageMultiplier;
                projectileBounces += definition.ProjectileBounces;
            }

            ShotProfile shotProfile = ShotProfile.Create(
                projectileCount,
                burstCount,
                spreadAngleDegrees,
                projectileSpeedMultiplier,
                projectileDamageMultiplier,
                projectileBounces);

            return new CombatCardStats(
                magazineSize + shotProfile.AmmoCost - ShotProfile.Base.AmmoCost,
                Mathf.Max(0.25f, reloadSeconds),
                shieldActiveSeconds,
                Mathf.Max(0.25f, shieldCooldownSeconds),
                shotProfile,
                projectileBounces);
        }
    }
}
