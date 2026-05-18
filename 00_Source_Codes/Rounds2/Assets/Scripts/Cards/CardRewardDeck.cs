namespace Rounds2.Cards
{
    public static class CardRewardDeck
    {
        public static System.Collections.Generic.IReadOnlyList<CardId> Cards { get; } = BuildCards();

        public static CardId NextForLoss(int existingCardCount)
        {
            int index = existingCardCount % Cards.Count;
            return Cards[index];
        }

        private static CardId[] BuildCards()
        {
            CardId[] cards = new CardId[CardCatalog.All.Count];
            for (int i = 0; i < CardCatalog.All.Count; i++)
            {
                cards[i] = CardCatalog.All[i].Id;
            }

            return cards;
        }
    }
}
