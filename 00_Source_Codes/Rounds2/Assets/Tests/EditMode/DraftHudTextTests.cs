using NUnit.Framework;
using Rounds2.Cards;
using Rounds2.UI;
using System;
using System.IO;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class DraftHudTextTests
    {
        [Test]
        public void FormatChoiceOfferShowsThreeNumberedCards()
        {
            CardDraftOffer offer = CardDraftOffer.Create(existingCardCount: 0);

            string text = DraftHudText.FormatChoiceOffer(playerNumber: 1, offer);

            StringAssert.Contains("P1 CARD PICK", text);
            StringAssert.Contains("[1] Burst Shot - +1 burst, +1 mag", text);
            StringAssert.Contains("[2] Split Shot - +1 projectile, +1 mag", text);
            StringAssert.Contains("[3] Fast Reload - reload -15%", text);
            Assert.IsFalse(text.Contains("Extra Ammo"));
        }

        [Test]
        public void FormatChoiceOfferShowsCategoryStackingAndOwnedState()
        {
            CardDraftOffer offer = CardDraftOffer.Create(existingCardCount: 7);
            CardId[] ownedCards =
            {
                CardId.Ricochet,
                CardId.BurstShot,
                CardId.BurstShot
            };

            string text = DraftHudText.FormatChoiceOffer(playerNumber: 1, offer, ownedCards);

            StringAssert.Contains("[1] Ricochet - +1 bullet bounce | Projectile | Stack | Owned x1", text);
            StringAssert.Contains("[2] Burst Shot - +1 burst, +1 mag | ShotCount | Stack | Owned x2", text);
            StringAssert.Contains("[3] Split Shot - +1 projectile, +1 mag | ShotCount | Stack | New", text);
        }

        [Test]
        public void FormatRewardShowsAcquiredCardAsSingleCard()
        {
            CardId[] ownedCards =
            {
                CardId.BurstShot,
                CardId.BurstShot,
                CardId.BurstShot
            };

            string text = DraftHudText.FormatReward(playerNumber: 1, CardId.BurstShot, ownedCards);

            StringAssert.Contains("P1 CARD GET", text);
            StringAssert.Contains("Burst Shot - +1 burst, +1 mag | ShotCount | Stack | Owned x3", text);
            Assert.AreEqual(2, text.Split('\n').Length);
        }

        [Test]
        public void DraftHudBuildsThreeCardPanelsForChoices()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "UI", "DraftHud.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("DraftCardPanel", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("CreateCardPanel", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("HorizontalLayoutGroup", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("private readonly Text[] cardTexts", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("ApplyCardText", StringComparison.Ordinal));
        }

        [Test]
        public void DraftHudCardPanelsSubmitChoicesByClick()
        {
            string hudPath = Path.Combine(Application.dataPath, "Scripts", "UI", "DraftHud.cs");
            string controllerPath = Path.Combine(Application.dataPath, "Scripts", "Player", "PlayerController.cs");
            string hudSource = File.ReadAllText(hudPath);
            string controllerSource = File.ReadAllText(controllerPath);

            Assert.IsTrue(hudSource.Contains("public static event Action<int> ChoiceClicked", StringComparison.Ordinal));
            Assert.IsTrue(hudSource.Contains("Button button = panelObject.AddComponent<Button>()", StringComparison.Ordinal));
            Assert.IsTrue(hudSource.Contains("ChoiceClicked?.Invoke(choiceIndex)", StringComparison.Ordinal));
            Assert.IsTrue(controllerSource.Contains("DraftHud.ChoiceClicked += SubmitDraftChoice", StringComparison.Ordinal));
            Assert.IsTrue(controllerSource.Contains("DraftHud.ChoiceClicked -= SubmitDraftChoice", StringComparison.Ordinal));
        }

        [Test]
        public void SetManagerShowsRewardCardUntilRoundReset()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Match", "SetManager.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("rewardHudText", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("DraftHudText.FormatReward", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("rewardHudText = string.Empty", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains(": rewardHudText", StringComparison.Ordinal));
        }
    }
}
