namespace Rounds2.Cards
{
    public static class CardText
    {
        public static string Name(CardId card)
        {
            return CardCatalog.Get(card).Name;
        }

        public static string Effect(CardId card)
        {
            return CardCatalog.Get(card).Effect;
        }

        public static string NameWithEffect(CardId card)
        {
            string effect = Effect(card);
            return string.IsNullOrEmpty(effect) ? Name(card) : $"{Name(card)} - {effect}";
        }
    }
}
