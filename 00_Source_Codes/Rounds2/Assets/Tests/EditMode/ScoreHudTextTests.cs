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
        public void FormatShowsCurrentSetWinsWhenProvided()
        {
            string text = ScoreHudText.Format(
                leftRoundWins: 2,
                rightRoundWins: 1,
                leftSetWins: 1,
                rightSetWins: 0,
                setWinsToWinRound: 2,
                roundWinsToWinMatch: 5,
                matchFinished: false,
                winnerLabel: "");

            Assert.AreEqual("Rounds 2 - 1\nSets 1 - 0 / 2\nFirst to 5", text);
        }

        [Test]
        public void FormatShowsWinnerWhenMatchFinished()
        {
            string text = ScoreHudText.Format(leftWins: 5, rightWins: 3, winsToWin: 5, matchFinished: true, winnerLabel: "P1");

            Assert.AreEqual("Rounds 5 - 3\nWinner: P1", text);
        }

        [Test]
        public void FormatCanShowLatestCardReward()
        {
            string text = ScoreHudText.Format(
                leftWins: 1,
                rightWins: 0,
                winsToWin: 5,
                matchFinished: false,
                winnerLabel: "",
                latestReward: "P2 gained Burst Shot - +1 burst, +1 mag");

            Assert.AreEqual("Rounds 1 - 0\nFirst to 5\nP2 gained Burst Shot - +1 burst, +1 mag", text);
        }
    }
}
