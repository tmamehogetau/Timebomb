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
        public void RecordSetWinRequiresTargetSetWinsBeforeRoundWin()
        {
            RoundScoreState score = new();
            int playerA = score.AddPlayer();
            int playerB = score.AddPlayer();

            bool roundFinished = score.RecordSetWin(
                playerA,
                MatchTuning.SetWinsToWinRound,
                MatchTuning.RoundWinsToWinMatch);

            Assert.IsFalse(roundFinished);
            Assert.AreEqual(1, score.GetSetWins(playerA));
            Assert.AreEqual(0, score.GetSetWins(playerB));
            Assert.AreEqual(0, score.GetRoundWins(playerA));
            Assert.AreEqual(0, score.GetRoundWins(playerB));
        }

        [Test]
        public void RecordSetWinConvertsTargetSetWinsIntoRoundWinAndClearsSetWins()
        {
            RoundScoreState score = new();
            int playerA = score.AddPlayer();
            int playerB = score.AddPlayer();

            score.RecordSetWin(playerA, MatchTuning.SetWinsToWinRound, MatchTuning.RoundWinsToWinMatch);
            bool roundFinished = score.RecordSetWin(
                playerA,
                MatchTuning.SetWinsToWinRound,
                MatchTuning.RoundWinsToWinMatch);

            Assert.IsTrue(roundFinished);
            Assert.AreEqual(0, score.GetSetWins(playerA));
            Assert.AreEqual(0, score.GetSetWins(playerB));
            Assert.AreEqual(1, score.GetRoundWins(playerA));
            Assert.AreEqual(0, score.GetRoundWins(playerB));
        }

        [Test]
        public void RecordSetWinFinishesMatchAtTargetRoundWins()
        {
            RoundScoreState score = new();
            int playerA = score.AddPlayer();
            score.AddPlayer();

            for (int i = 0; i < MatchTuning.RoundWinsToWinMatch * MatchTuning.SetWinsToWinRound; i++)
            {
                score.RecordSetWin(playerA, MatchTuning.SetWinsToWinRound, MatchTuning.RoundWinsToWinMatch);
            }

            Assert.IsTrue(score.IsMatchFinished);
            Assert.AreEqual(playerA, score.MatchWinnerIndex);
            Assert.AreEqual(MatchTuning.RoundWinsToWinMatch, score.GetRoundWins(playerA));
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

            score.RecordSetWin(playerA, MatchTuning.SetWinsToWinRound, MatchTuning.RoundWinsToWinMatch);
            score.RecordRoundWin(playerA, MatchTuning.RoundWinsToWinMatch);
            score.RecordRoundWin(playerB, MatchTuning.RoundWinsToWinMatch);
            score.ResetMatch();

            Assert.IsFalse(score.IsMatchFinished);
            Assert.AreEqual(-1, score.MatchWinnerIndex);
            Assert.AreEqual(0, score.GetSetWins(playerA));
            Assert.AreEqual(0, score.GetSetWins(playerB));
            Assert.AreEqual(0, score.GetRoundWins(playerA));
            Assert.AreEqual(0, score.GetRoundWins(playerB));
        }
    }
}
