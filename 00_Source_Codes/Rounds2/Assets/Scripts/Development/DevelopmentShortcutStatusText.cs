namespace Rounds2.Development
{
    public static class DevelopmentShortcutStatusText
    {
        public static string Format(DevelopmentShortcutState state)
        {
            string botState = state != null && state.BotEnabled ? "BOT ON" : "BOT OFF";
            return $"DEV  F2 {botState}  F5 RESET";
        }
    }
}
