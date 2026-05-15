using NUnit.Framework;
using Rounds2.Match;

namespace Rounds2.Tests.EditMode
{
    public sealed class SetManagerRulesTests
    {
        [Test]
        public void TryFindWinnerReturnsNoWinnerWithTwoAlivePlayers()
        {
            bool foundWinner = SetRules.TryFindWinner(new[] { false, false }, out int winnerIndex);

            Assert.IsFalse(foundWinner);
            Assert.AreEqual(-1, winnerIndex);
        }

        [Test]
        public void TryFindWinnerReturnsOnlyAlivePlayer()
        {
            bool foundWinner = SetRules.TryFindWinner(new[] { true, false }, out int winnerIndex);

            Assert.IsTrue(foundWinner);
            Assert.AreEqual(1, winnerIndex);
        }

        [Test]
        public void TryFindWinnerReturnsNoWinnerWhenAllPlayersAreDead()
        {
            bool foundWinner = SetRules.TryFindWinner(new[] { true, true }, out int winnerIndex);

            Assert.IsFalse(foundWinner);
            Assert.AreEqual(-1, winnerIndex);
        }
    }
}
