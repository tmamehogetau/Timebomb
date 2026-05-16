namespace Rounds2.Development
{
    public sealed class DevelopmentShortcutState
    {
        public DevelopmentShortcutState(bool initialBotEnabled)
        {
            BotEnabled = initialBotEnabled;
        }

        public bool BotEnabled { get; private set; }

        public bool ToggleBot()
        {
            BotEnabled = !BotEnabled;
            return BotEnabled;
        }
    }
}
