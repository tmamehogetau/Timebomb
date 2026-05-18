using System.Collections.Generic;
using Rounds2.Cards;

namespace Rounds2.UI
{
    public static class DraftHudText
    {
        public static string FormatChoiceOffer(int playerNumber, CardDraftOffer offer)
        {
            return FormatChoiceOffer(playerNumber, offer, ownedCards: null);
        }

        public static string FormatChoiceOffer(int playerNumber, CardDraftOffer offer, IReadOnlyList<CardId> ownedCards)
        {
            return $"P{playerNumber} CARD PICK\n{FormatChoice(1, offer.GetChoice(0), ownedCards)}\n{FormatChoice(2, offer.GetChoice(1), ownedCards)}\n{FormatChoice(3, offer.GetChoice(2), ownedCards)}";
        }

        public static string FormatReward(int playerNumber, CardId card, IReadOnlyList<CardId> ownedCards)
        {
            return $"P{playerNumber} CARD GET\n{FormatCard(card, ownedCards)}";
        }

        private static string FormatChoice(int choiceNumber, CardId card, IReadOnlyList<CardId> ownedCards)
        {
            return $"[{choiceNumber}] {FormatCard(card, ownedCards)}";
        }

        private static string FormatCard(CardId card, IReadOnlyList<CardId> ownedCards)
        {
            CardDefinition definition = CardCatalog.Get(card);
            return $"{CardText.NameWithEffect(card)} | {definition.Category} | {StackingText(definition)} | {OwnedText(definition, card, ownedCards)}";
        }

        private static string StackingText(CardDefinition definition)
        {
            return definition.Stacking == CardStacking.Cumulative ? "Stack" : "Unique";
        }

        private static string OwnedText(CardDefinition definition, CardId card, IReadOnlyList<CardId> ownedCards)
        {
            int ownedCount = CountOwned(card, ownedCards);
            if (definition.Stacking == CardStacking.NonCumulative)
            {
                return ownedCount > 0 ? "Owned" : "New";
            }

            return ownedCount > 0 ? $"Owned x{ownedCount}" : "New";
        }

        private static int CountOwned(CardId card, IReadOnlyList<CardId> ownedCards)
        {
            if (ownedCards == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < ownedCards.Count; i++)
            {
                if (ownedCards[i] == card)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
