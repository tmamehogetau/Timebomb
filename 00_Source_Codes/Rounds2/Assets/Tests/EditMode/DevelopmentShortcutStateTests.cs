using NUnit.Framework;
using Rounds2.Development;
using Rounds2.Cards;
using System;
using System.IO;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class DevelopmentShortcutStateTests
    {
        [Test]
        public void ToggleBotFlipsCurrentBotState()
        {
            DevelopmentShortcutState state = new(initialBotEnabled: false);

            bool enabled = state.ToggleBot();

            Assert.IsTrue(enabled);
            Assert.IsTrue(state.BotEnabled);
        }

        [Test]
        public void StatusTextShowsBotAndResetShortcut()
        {
            DevelopmentShortcutState state = new(initialBotEnabled: true);

            string text = DevelopmentShortcutStatusText.Format(state);

            StringAssert.Contains("BOT ON", text);
            StringAssert.Contains("F2", text);
            StringAssert.Contains("F5", text);
            StringAssert.Contains("F6", text);
            StringAssert.Contains("F7", text);
            StringAssert.Contains("F8", text);
            StringAssert.Contains(CardText.Name(CardId.BurstShot), text);
        }

        [Test]
        public void CardShortcutSelectionCyclesThroughCatalog()
        {
            DevelopmentShortcutState state = new(initialBotEnabled: false);

            Assert.AreEqual(CardId.BurstShot, state.SelectedCard);

            CardId selected = state.SelectNextCard();

            Assert.AreEqual(CardId.SplitShot, selected);
            Assert.AreEqual(CardId.SplitShot, state.SelectedCard);
        }

        [Test]
        public void CardStatsTextSummarizesEffectiveCombatStats()
        {
            PlayerCardCollection cards = new();
            cards.Grant(CardId.BurstShot);
            cards.Grant(CardId.SplitShot);
            CombatCardStats stats = cards.BuildStats();

            string text = DevelopmentCardStatsText.Format(stats, cards.Count);

            StringAssert.Contains("CARDS 2", text);
            StringAssert.Contains("mag 9", text);
            StringAssert.Contains("reload 1.50s", text);
            StringAssert.Contains("shot 2x2", text);
            StringAssert.Contains("cost 4", text);
            StringAssert.Contains("spread 10", text);
            StringAssert.Contains("dmg x0.50", text);
            StringAssert.Contains("speed x1.00", text);
            StringAssert.Contains("bounce 0", text);
            StringAssert.Contains("shield 0.35s/4.00s", text);
        }

        [Test]
        public void ShortcutOverlayStaysAwayFromTopLeftOverlays()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Development", "PlayerDevelopmentShortcuts.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("Screen.height - 44f", StringComparison.Ordinal));
        }

        [Test]
        public void PlayerDevelopmentShortcutsCanGrantAndClearSelectedCards()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Development", "PlayerDevelopmentShortcuts.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("keyboard.f6Key.wasPressedThisFrame", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("keyboard.f7Key.wasPressedThisFrame", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("keyboard.f8Key.wasPressedThisFrame", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("RequestGrantCardServerRpc(DevelopmentRuntimeOptions.SelectedCard)", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("RequestClearCardsServerRpc()", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("loadout.Grant(card)", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("loadout.Clear()", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("RefreshCardStats()", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("DevelopmentCardStatsText.Format(loadout.Stats, loadout.CardCount)", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("SetCardStatsTextTargetRpc(Owner, summary)", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("[TargetRpc]", StringComparison.Ordinal));
        }
    }
}
