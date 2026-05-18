using System;
using System.Collections.Generic;

namespace Rounds2.Cards
{
    public static class CardCatalog
    {
        private static readonly CardDefinition[] Definitions =
        {
            new(
                CardId.BurstShot,
                "Burst Shot",
                "+1 burst, +1 mag",
                CardCategory.ShotCount,
                CardStacking.Cumulative,
                burstCountDelta: 1),
            new(
                CardId.SplitShot,
                "Split Shot",
                "+1 projectile, +1 mag",
                CardCategory.ShotCount,
                CardStacking.Cumulative,
                projectileCountDelta: 1,
                spreadAngleDegreesDelta: 10f),
            new(
                CardId.FastReload,
                "Fast Reload",
                "reload -15%",
                CardCategory.Reload,
                CardStacking.Cumulative,
                reloadMultiplier: 0.85f),
            new(
                CardId.LongShield,
                "Long Shield",
                "shield +0.15s",
                CardCategory.Defense,
                CardStacking.Cumulative,
                shieldActiveSecondsDelta: 0.15f),
            new(
                CardId.ShieldCoolant,
                "Shield Coolant",
                "shield cooldown -15%",
                CardCategory.Defense,
                CardStacking.Cumulative,
                shieldCooldownMultiplier: 0.85f),
            new(
                CardId.QuickRounds,
                "Quick Rounds",
                "bullet speed +15%",
                CardCategory.Projectile,
                CardStacking.Cumulative,
                projectileSpeedMultiplier: 1.15f),
            new(
                CardId.HeavyRounds,
                "Heavy Rounds",
                "damage +1, speed -8%",
                CardCategory.Projectile,
                CardStacking.Cumulative,
                projectileSpeedMultiplier: 0.92f,
                projectileDamageMultiplier: 1.04f),
            new(
                CardId.Ricochet,
                "Ricochet",
                "+1 bullet bounce",
                CardCategory.Projectile,
                CardStacking.Cumulative,
                projectileBounces: 1)
        };

        public static IReadOnlyList<CardDefinition> All => Definitions;

        public static CardDefinition Get(CardId card)
        {
            foreach (CardDefinition definition in Definitions)
            {
                if (definition.Id == card)
                {
                    return definition;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(card), card, "Unknown card id.");
        }
    }
}
