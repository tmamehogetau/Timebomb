using Rounds2.Cards;

namespace Rounds2.Development
{
    public static class DevelopmentShortcutStatusText
    {
        public static string Format(DevelopmentShortcutState state)
        {
            string botState = state != null && state.BotEnabled ? "BOT ON" : "BOT OFF";
            string selectedCard = state != null ? CardText.Name(state.SelectedCard) : CardText.Name(CardId.BurstShot);
            return $"DEV  F2 {botState}  F5 RESET  F6 CARD {selectedCard}  F7 NEXT  F8 CLEAR";
        }
    }
}
