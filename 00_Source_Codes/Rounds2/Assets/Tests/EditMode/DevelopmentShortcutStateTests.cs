using NUnit.Framework;
using Rounds2.Development;
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
        }

        [Test]
        public void ShortcutOverlayStaysAwayFromTopLeftOverlays()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Development", "PlayerDevelopmentShortcuts.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("Screen.height - 44f", StringComparison.Ordinal));
        }
    }
}
