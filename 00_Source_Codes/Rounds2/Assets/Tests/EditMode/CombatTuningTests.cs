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
        public void MatchUsesFiveRoundWinsAndTwoSetWins()
        {
            Assert.AreEqual(5, MatchTuning.RoundWinsToWinMatch);
            Assert.AreEqual(2, MatchTuning.SetWinsToWinRound);
        }
    }
}
