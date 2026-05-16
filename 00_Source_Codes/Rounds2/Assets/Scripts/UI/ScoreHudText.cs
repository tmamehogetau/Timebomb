namespace Rounds2.UI
{
    public static class ScoreHudText
    {
        public static string Format(int leftWins, int rightWins, int winsToWin, bool matchFinished, string winnerLabel)
        {
            string status = matchFinished ? $"Winner: {winnerLabel}" : $"First to {winsToWin}";
            return $"Rounds {leftWins} - {rightWins}\n{status}";
        }
    }
}
