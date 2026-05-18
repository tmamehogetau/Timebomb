using Rounds2.Cards;

namespace Rounds2.Development
{
    public sealed class DevelopmentShortcutState
    {
        private int selectedCardIndex;

        public DevelopmentShortcutState(bool initialBotEnabled)
        {
            BotEnabled = initialBotEnabled;
        }

        public bool BotEnabled { get; private set; }
        public CardId SelectedCard => CardCatalog.All[selectedCardIndex].Id;

        public bool ToggleBot()
        {
            BotEnabled = !BotEnabled;
            return BotEnabled;
        }

        public CardId SelectNextCard()
        {
            selectedCardIndex = (selectedCardIndex + 1) % CardCatalog.All.Count;
            return SelectedCard;
        }
    }
}
