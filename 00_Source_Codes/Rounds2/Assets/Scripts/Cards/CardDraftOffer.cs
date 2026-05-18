using System;

namespace Rounds2.Cards
{
    public readonly struct CardDraftOffer
    {
        private const int ChoiceCount = 3;
        private readonly CardId first;
        private readonly CardId second;
        private readonly CardId third;

        private CardDraftOffer(CardId first, CardId second, CardId third)
        {
            this.first = first;
            this.second = second;
            this.third = third;
        }

        public static CardDraftOffer Create(int existingCardCount)
        {
            return new CardDraftOffer(
                CardRewardDeck.NextForLoss(existingCardCount),
                CardRewardDeck.NextForLoss(existingCardCount + 1),
                CardRewardDeck.NextForLoss(existingCardCount + 2));
        }

        public CardId GetChoice(int choiceIndex)
        {
            return choiceIndex switch
            {
                0 => first,
                1 => second,
                2 => third,
                _ => throw new ArgumentOutOfRangeException(nameof(choiceIndex))
            };
        }

        public string FormatChoices()
        {
            return $"1 {CardText.Name(first)} / 2 {CardText.Name(second)} / 3 {CardText.Name(third)}";
        }

        public static int Count => ChoiceCount;
    }
}
