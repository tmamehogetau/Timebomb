using Rounds2.Networking;

using Rounds2.Cards;

namespace Rounds2.Development
{
    public static class DevelopmentRuntimeOptions
    {
        private static DevelopmentShortcutState state;

        public static bool BotEnabled => State.BotEnabled;
        public static CardId SelectedCard => State.SelectedCard;
        public static string StatusText => DevelopmentShortcutStatusText.Format(State);

        private static DevelopmentShortcutState State
        {
            get
            {
                state ??= new DevelopmentShortcutState(BootstrapLaunchOptions.BotEnabled(System.Environment.GetCommandLineArgs()));
                return state;
            }
        }

        public static bool ToggleBot()
        {
            return State.ToggleBot();
        }

        public static CardId SelectNextCard()
        {
            return State.SelectNextCard();
        }
    }
}
