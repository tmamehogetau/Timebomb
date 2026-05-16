using NUnit.Framework;
using Rounds2.UI;

namespace Rounds2.Tests.EditMode
{
    public sealed class ScoreHudTextTests
    {
        [Test]
        public void FormatShowsRoundWinsAndTarget()
        {
            string text = ScoreHudText.Format(leftWins: 2, rightWins: 1, winsToWin: 5, matchFinished: false, winnerLabel: "");

            Assert.AreEqual("Rounds 2 - 1\nFirst to 5", text);
        }

        [Test]
        public void FormatShowsWinnerWhenMatchFinished()
        {
            string text = ScoreHudText.Format(leftWins: 5, rightWins: 3, winsToWin: 5, matchFinished: true, winnerLabel: "P1");

            Assert.AreEqual("Rounds 5 - 3\nWinner: P1", text);
        }
    }
}
