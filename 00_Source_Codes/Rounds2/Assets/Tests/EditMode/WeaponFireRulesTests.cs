using NUnit.Framework;
using Rounds2.Combat;

namespace Rounds2.Tests.EditMode
{
    public sealed class WeaponFireRulesTests
    {
        [Test]
        public void CanFireAllowsLivingPlayerDuringOpenCombat()
        {
            Assert.IsTrue(WeaponFireRules.CanFire(roundCombatOpen: true, shooterDead: false));
        }

        [Test]
        public void CanFireBlocksDuringRoundTransition()
        {
            Assert.IsFalse(WeaponFireRules.CanFire(roundCombatOpen: false, shooterDead: false));
        }

        [Test]
        public void CanFireBlocksDeadShooter()
        {
            Assert.IsFalse(WeaponFireRules.CanFire(roundCombatOpen: true, shooterDead: true));
        }
    }
}
