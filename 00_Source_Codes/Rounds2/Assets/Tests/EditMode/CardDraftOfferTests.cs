using NUnit.Framework;
using Rounds2.Cards;

namespace Rounds2.Tests.EditMode
{
    public sealed class CardDraftOfferTests
    {
        [Test]
        public void CreateOffersThreeCardsStartingAtExistingCardCount()
        {
            CardDraftOffer offer = CardDraftOffer.Create(existingCardCount: 1);

            Assert.AreEqual(CardId.SplitShot, offer.GetChoice(0));
            Assert.AreEqual(CardId.FastReload, offer.GetChoice(1));
            Assert.AreEqual(CardId.LongShield, offer.GetChoice(2));
        }

        [Test]
        public void ChoicePromptShowsNumberedCardsForHud()
        {
            CardDraftOffer offer = CardDraftOffer.Create(existingCardCount: 0);

            Assert.AreEqual("1 Burst Shot / 2 Split Shot / 3 Fast Reload", offer.FormatChoices());
        }

        [Test]
        public void CardTextUsesShortReadableNamesAndEffects()
        {
            Assert.AreEqual("Burst Shot", CardText.Name(CardId.BurstShot));
            Assert.AreEqual("+1 burst, +1 mag", CardText.Effect(CardId.BurstShot));
            Assert.AreEqual("Burst Shot - +1 burst, +1 mag", CardText.NameWithEffect(CardId.BurstShot));
            Assert.AreEqual("Fast Reload - reload -15%", CardText.NameWithEffect(CardId.FastReload));
        }
    }
}
