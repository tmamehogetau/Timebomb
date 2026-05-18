using System.IO;
using System.Linq;
using NUnit.Framework;
using Rounds2.Cards;

namespace Rounds2.Tests.EditMode
{
    public sealed class CardDefinitionTests
    {
        [Test]
        public void CatalogDefinesFullMvpCardList()
        {
            CollectionAssert.AreEqual(
                new[]
                {
                    "BurstShot",
                    "SplitShot",
                    "FastReload",
                    "LongShield",
                    "ShieldCoolant",
                    "QuickRounds",
                    "HeavyRounds",
                    "Ricochet"
                },
                CardCatalog.All.Select(card => card.Id.ToString()).ToArray());

            CardDefinition burst = CardCatalog.Get(CardId.BurstShot);
            Assert.AreEqual("Burst Shot", burst.Name);
            Assert.AreEqual("+1 burst, +1 mag", burst.Effect);
            Assert.AreEqual(CardCategory.ShotCount, burst.Category);
            Assert.AreEqual(CardStacking.Cumulative, burst.Stacking);
            Assert.AreEqual(1, burst.BurstCountDelta);

            CardDefinition reload = CardCatalog.Get(CardId.FastReload);
            Assert.AreEqual(CardCategory.Reload, reload.Category);
            Assert.AreEqual(0.85f, reload.ReloadMultiplier, 0.001f);

            CardDefinition ricochet = CardCatalog.All.Single(card => card.Id.ToString() == "Ricochet");
            Assert.AreEqual(CardStacking.Cumulative, ricochet.Stacking);
            Assert.AreEqual("Projectile", ricochet.Category.ToString());
        }

        [Test]
        public void RewardDeckUsesCatalogOrder()
        {
            CollectionAssert.AreEqual(
                CardCatalog.All.Select(card => card.Id),
                CardRewardDeck.Cards);
        }

        [Test]
        public void CardTextAndLoadoutUseCardDefinitions()
        {
            string cardTextSource = File.ReadAllText("Assets/Scripts/Cards/CardText.cs");
            string loadoutSource = File.ReadAllText("Assets/Scripts/Cards/PlayerCardCollection.cs");

            Assert.IsTrue(cardTextSource.Contains("CardCatalog.Get(card)"));
            Assert.IsTrue(loadoutSource.Contains("CardCatalog.Get(card)"));
            Assert.IsFalse(cardTextSource.Contains("CardId.BurstShot =>"));
            Assert.IsFalse(loadoutSource.Contains("case CardId."));
        }
    }
}
