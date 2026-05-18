using NUnit.Framework;
using Rounds2.Cards;
using Rounds2.Config;
using System;
using System.Reflection;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class CardLoadoutTests
    {
        [Test]
        public void CollectionBuildsStackedCombatStats()
        {
            PlayerCardCollection cards = new();

            cards.Grant(CardId.FastReload);
            cards.Grant(CardId.LongShield);
            cards.Grant(CardId.BurstShot);
            cards.Grant(CardId.BurstShot);
            cards.Grant(CardId.SplitShot);
            cards.Grant(CardId.SplitShot);

            CombatCardStats stats = cards.BuildStats();

            Assert.AreEqual(CombatTuning.MagazineSize + 8, stats.MagazineSize);
            Assert.AreEqual(CombatTuning.ReloadSeconds * 0.85f, stats.ReloadSeconds, 0.001f);
            Assert.AreEqual(CombatTuning.ShieldActiveSeconds + 0.15f, stats.ShieldActiveSeconds, 0.001f);
            Assert.AreEqual(CombatTuning.ShieldCooldownSeconds, stats.ShieldCooldownSeconds, 0.001f);
            Assert.AreEqual(3, stats.ShotProfile.BurstCount);
            Assert.AreEqual(3, stats.ShotProfile.ProjectileCount);
            Assert.AreEqual(20f, stats.ShotProfile.SpreadAngleDegrees, 0.001f);
            Assert.AreEqual(9, stats.ShotProfile.AmmoCost);
            Assert.Less(stats.ShotProfile.DamageMultiplier, 1f);
            Assert.Greater(stats.ShotProfile.DamageMultiplier, 0.1f);
        }

        [Test]
        public void LoadoutExposesGrantedCardsAndClearsThem()
        {
            GameObject player = new("Player");
            PlayerCardLoadout loadout = player.AddComponent<PlayerCardLoadout>();

            loadout.Grant(CardId.BurstShot);
            loadout.Grant(CardId.FastReload);

            CollectionAssert.AreEqual(new[] { CardId.BurstShot, CardId.FastReload }, loadout.Cards);

            loadout.Clear();

            Assert.AreEqual(0, loadout.Cards.Count);
            UnityEngine.Object.DestroyImmediate(player);
        }

        [Test]
        public void RewardDeckCyclesThroughPrototypeCards()
        {
            Assert.AreEqual(CardId.BurstShot, CardRewardDeck.NextForLoss(0));
            Assert.AreEqual(CardId.SplitShot, CardRewardDeck.NextForLoss(1));
            Assert.AreEqual(CardId.FastReload, CardRewardDeck.NextForLoss(2));
            Assert.AreEqual(CardId.LongShield, CardRewardDeck.NextForLoss(3));
            Assert.AreEqual(CardId.ShieldCoolant, CardRewardDeck.NextForLoss(4));
            Assert.AreEqual(CardId.QuickRounds, CardRewardDeck.NextForLoss(5));
            Assert.AreEqual(CardId.HeavyRounds, CardRewardDeck.NextForLoss(6));
            Assert.AreEqual(CardId.Ricochet, CardRewardDeck.NextForLoss(7));
            Assert.AreEqual(CardId.BurstShot, CardRewardDeck.NextForLoss(8));
        }

        [Test]
        public void RewardDeckDoesNotOfferStandaloneAmmoCards()
        {
            string source = System.IO.File.ReadAllText("Assets/Scripts/Cards/CardRewardDeck.cs");

            Assert.IsFalse(source.Contains("ExtraAmmo"));
        }

        [Test]
        public void ShotProfileKeepsProjectileCapsHighAndScalesDamageByTotalProjectiles()
        {
            PlayerCardCollection cards = new();
            for (int i = 0; i < 20; i++)
            {
                cards.Grant(CardId.BurstShot);
                cards.Grant(CardId.SplitShot);
            }

            ShotProfile profile = cards.BuildStats().ShotProfile;

            Assert.AreEqual(15, profile.BurstCount);
            Assert.AreEqual(21, profile.ProjectileCount);
            Assert.AreEqual(90f, profile.SpreadAngleDegrees, 0.001f);
            Assert.AreEqual(315, profile.AmmoCost);
            Assert.AreEqual(0.18f, profile.DamageMultiplier, 0.001f);
        }

        [Test]
        public void ShotCountCardsIncreaseMagazineEnoughForTheirFullShotCost()
        {
            PlayerCardCollection cards = new();

            cards.Grant(CardId.BurstShot);
            cards.Grant(CardId.SplitShot);

            CombatCardStats stats = cards.BuildStats();

            Assert.AreEqual(4, stats.ShotProfile.AmmoCost);
            Assert.AreEqual(CombatTuning.MagazineSize + 3, stats.MagazineSize);
            Assert.GreaterOrEqual(stats.MagazineSize, stats.ShotProfile.AmmoCost);
        }

        [Test]
        public void MvpCardsBuildProjectileDefenseAndStackingStats()
        {
            PlayerCardCollection cards = new();
            cards.Grant(ParseCard("ShieldCoolant"));
            cards.Grant(ParseCard("ShieldCoolant"));
            cards.Grant(ParseCard("QuickRounds"));
            cards.Grant(ParseCard("QuickRounds"));
            cards.Grant(ParseCard("HeavyRounds"));
            cards.Grant(ParseCard("Ricochet"));
            cards.Grant(ParseCard("Ricochet"));

            CombatCardStats stats = cards.BuildStats();

            Assert.AreEqual(CombatTuning.ShieldCooldownSeconds * 0.85f * 0.85f, stats.ShieldCooldownSeconds, 0.001f);
            Assert.AreEqual(2, GetIntProperty(stats, "ProjectileBounces"));
            Assert.AreEqual(1.15f * 1.15f * 0.92f, stats.ShotProfile.SpeedMultiplier, 0.001f);
            Assert.Greater(stats.ShotProfile.DamageMultiplier, 1f);
            Assert.Less(stats.ShotProfile.DamageMultiplier, 1.1f);
        }

        private static CardId ParseCard(string name)
        {
            return (CardId)Enum.Parse(typeof(CardId), name);
        }

        private static int GetIntProperty(CombatCardStats stats, string propertyName)
        {
            PropertyInfo property = typeof(CombatCardStats).GetProperty(propertyName);
            Assert.IsNotNull(property);
            return (int)property.GetValue(stats);
        }
    }
}
