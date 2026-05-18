namespace Rounds2.UI
{
    public static class ScoreHudText
    {
        public static string Format(int leftWins, int rightWins, int winsToWin, bool matchFinished, string winnerLabel)
        {
            return Format(leftWins, rightWins, winsToWin, matchFinished, winnerLabel, string.Empty);
        }

        public static string Format(int leftWins, int rightWins, int winsToWin, bool matchFinished, string winnerLabel, string latestReward)
        {
            string status = matchFinished ? $"Winner: {winnerLabel}" : $"First to {winsToWin}";
            string rewardLine = string.IsNullOrEmpty(latestReward) ? string.Empty : $"\n{latestReward}";
            return $"Rounds {leftWins} - {rightWins}\n{status}{rewardLine}";
        }

        public static string Format(
            int leftRoundWins,
            int rightRoundWins,
            int leftSetWins,
            int rightSetWins,
            int setWinsToWinRound,
            int roundWinsToWinMatch,
            bool matchFinished,
            string winnerLabel)
        {
            return Format(
                leftRoundWins,
                rightRoundWins,
                leftSetWins,
                rightSetWins,
                setWinsToWinRound,
                roundWinsToWinMatch,
                matchFinished,
                winnerLabel,
                string.Empty);
        }

        public static string Format(
            int leftRoundWins,
            int rightRoundWins,
            int leftSetWins,
            int rightSetWins,
            int setWinsToWinRound,
            int roundWinsToWinMatch,
            bool matchFinished,
            string winnerLabel,
            string latestReward)
        {
            string status = matchFinished ? $"Winner: {winnerLabel}" : $"First to {roundWinsToWinMatch}";
            string rewardLine = string.IsNullOrEmpty(latestReward) ? string.Empty : $"\n{latestReward}";
            return $"Rounds {leftRoundWins} - {rightRoundWins}\nSets {leftSetWins} - {rightSetWins} / {setWinsToWinRound}\n{status}{rewardLine}";
        }
    }
}
