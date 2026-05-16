using NUnit.Framework;
using Rounds2.Config;
using Rounds2.Match;

namespace Rounds2.Tests.EditMode
{
    public sealed class RoundScoreStateTests
    {
        [Test]
        public void AddPlayerStartsWithZeroRoundWins()
        {
            RoundScoreState score = new();

            int playerIndex = score.AddPlayer();

            Assert.AreEqual(0, playerIndex);
            Assert.AreEqual(0, score.GetRoundWins(playerIndex));
        }

        [Test]
        public void RecordRoundWinAddsOneWinToWinnerOnly()
        {
            RoundScoreState score = new();
            int playerA = score.AddPlayer();
            int playerB = score.AddPlayer();

            score.RecordRoundWin(playerB, MatchTuning.RoundWinsToWinMatch);

            Assert.AreEqual(0, score.GetRoundWins(playerA));
            Assert.AreEqual(1, score.GetRoundWins(playerB));
        }

        [Test]
        public void RecordRoundWinFinishesMatchAtTargetRoundWins()
        {
            RoundScoreState score = new();
            int playerA = score.AddPlayer();
            score.AddPlayer();

            for (int i = 0; i < MatchTuning.RoundWinsToWinMatch; i++)
            {
                score.RecordRoundWin(playerA, MatchTuning.RoundWinsToWinMatch);
            }

            Assert.IsTrue(score.IsMatchFinished);
            Assert.AreEqual(playerA, score.MatchWinnerIndex);
            Assert.AreEqual(MatchTuning.RoundWinsToWinMatch, score.GetRoundWins(playerA));
        }

        [Test]
        public void ResetMatchClearsRoundWinsAndWinner()
        {
            RoundScoreState score = new();
            int playerA = score.AddPlayer();
            int playerB = score.AddPlayer();

            score.RecordRoundWin(playerA, MatchTuning.RoundWinsToWinMatch);
            score.RecordRoundWin(playerB, MatchTuning.RoundWinsToWinMatch);
            score.ResetMatch();

            Assert.IsFalse(score.IsMatchFinished);
            Assert.AreEqual(-1, score.MatchWinnerIndex);
            Assert.AreEqual(0, score.GetRoundWins(playerA));
            Assert.AreEqual(0, score.GetRoundWins(playerB));
        }
    }
}
