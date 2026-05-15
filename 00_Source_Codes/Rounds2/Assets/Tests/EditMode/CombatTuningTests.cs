using NUnit.Framework;
using Rounds2.Config;

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
        public void MatchUsesFiveRoundWinsAndTwoSetWins()
        {
            Assert.AreEqual(5, MatchTuning.RoundWinsToWinMatch);
            Assert.AreEqual(2, MatchTuning.SetWinsToWinRound);
        }
    }
}
