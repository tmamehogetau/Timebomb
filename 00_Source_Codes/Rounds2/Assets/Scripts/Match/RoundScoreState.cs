using System.Collections.Generic;

namespace Rounds2.Match
{
    public sealed class RoundScoreState
    {
        private readonly List<int> roundWins = new();

        public bool IsMatchFinished { get; private set; }
        public int MatchWinnerIndex { get; private set; } = -1;

        public int AddPlayer()
        {
            roundWins.Add(0);
            return roundWins.Count - 1;
        }

        public int GetRoundWins(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= roundWins.Count)
            {
                return 0;
            }

            return roundWins[playerIndex];
        }

        public void RecordRoundWin(int winnerIndex, int roundWinsToWinMatch)
        {
            if (IsMatchFinished || winnerIndex < 0 || winnerIndex >= roundWins.Count)
            {
                return;
            }

            roundWins[winnerIndex]++;
            if (roundWins[winnerIndex] >= roundWinsToWinMatch)
            {
                IsMatchFinished = true;
                MatchWinnerIndex = winnerIndex;
            }
        }

        public void ResetMatch()
        {
            for (int i = 0; i < roundWins.Count; i++)
            {
                roundWins[i] = 0;
            }

            IsMatchFinished = false;
            MatchWinnerIndex = -1;
        }
    }
}
