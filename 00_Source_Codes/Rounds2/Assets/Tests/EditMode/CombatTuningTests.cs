using NUnit.Framework;
using Rounds2.Config;
using System.Reflection;

namespace Rounds2.Tests.EditMode
{
    public sealed class CombatTuningTests
    {
        [Test]
        public void VanillaWeaponKillsInFourHits()
        {
            Assert.AreEqual(4, CombatTuning.BaseHealth / CombatTuning.BulletDamage);
        }

        [Test]
        public void BulletKnockbackIsStrongerThanBaseMovement()
        {
            Assert.Greater(CombatTuning.BulletKnockbackSpeed, CombatTuning.MoveSpeed);
            Assert.Greater(CombatTuning.BulletKnockbackDurationTicks, 0u);
        }

        [Test]
        public void BulletLeakSafetyLifetimeIsNotWeaponRange()
        {
            FieldInfo field = typeof(CombatTuning).GetField(
                "BulletLeakSafetyLifetimeSeconds",
                BindingFlags.Public | BindingFlags.Static);

            Assert.IsNotNull(field);
            Assert.GreaterOrEqual((float)field.GetRawConstantValue(), 30f);
        }

        [Test]
        public void MovementInertiaKeepsArcadeResponseFast()
        {
            Assert.Greater(CombatTuning.MoveAcceleration, CombatTuning.MoveSpeed / 0.12f);
            Assert.Greater(CombatTuning.MoveDeceleration, CombatTuning.MoveSpeed / 0.16f);
        }

        [Test]
        public void AimIndicatorActsAsShortTurret()
        {
            Assert.AreEqual(0.75f, CombatTuning.AimIndicatorLength, 0.001f);
            Assert.AreEqual(
                CombatTuning.MuzzleForwardOffset + CombatTuning.AimIndicatorLength,
                CombatTuning.BulletSpawnForwardOffset,
                0.001f);
        }

        [Test]
        public void HitFeedbackStaysSubtle()
        {
            Assert.LessOrEqual(CombatTuning.DamageFeedbackSeconds, 0.12f);
            Assert.LessOrEqual(CombatTuning.DamageFeedbackWhiteBlend, 0.5f);
            Assert.LessOrEqual(CombatTuning.ShieldBlockFeedbackSeconds, 0.14f);
            Assert.LessOrEqual(CombatTuning.ShieldBlockFeedbackAlphaBoost, 0.3f);
            Assert.LessOrEqual(CombatTuning.ShieldBlockFeedbackScaleBoost, 0.15f);
        }

        [Test]
        public void BulletTrailStaysSubtle()
        {
            Assert.LessOrEqual(CombatTuning.BulletTrailSeconds, 0.1f);
            Assert.LessOrEqual(CombatTuning.BulletTrailStartWidth, 0.06f);
            Assert.AreEqual(0f, CombatTuning.BulletTrailEndWidth, 0.001f);
        }

        [Test]
        public void BulletWallImpactLingerStaysSubtle()
        {
            Assert.Greater(CombatTuning.BulletWallImpactLingerSeconds, 0f);
            Assert.LessOrEqual(CombatTuning.BulletWallImpactLingerSeconds, 0.1f);
        }

        [Test]
        public void MatchUsesFiveRoundWinsAndTwoSetWins()
        {
            Assert.AreEqual(5, MatchTuning.RoundWinsToWinMatch);
            Assert.AreEqual(2, MatchTuning.SetWinsToWinRound);
        }
    }
}
