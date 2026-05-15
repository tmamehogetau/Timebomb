using System.Collections.Generic;

namespace Rounds2.Match
{
    public static class SetRules
    {
        public static bool TryFindWinner(IReadOnlyList<bool> deadPlayers, out int winnerIndex)
        {
            winnerIndex = -1;
            int aliveCount = 0;

            for (int i = 0; i < deadPlayers.Count; i++)
            {
                if (deadPlayers[i])
                {
                    continue;
                }

                aliveCount++;
                winnerIndex = i;
            }

            if (aliveCount == 1)
            {
                return true;
            }

            winnerIndex = -1;
            return false;
        }
    }
}
