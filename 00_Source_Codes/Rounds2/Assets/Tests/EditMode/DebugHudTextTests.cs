using NUnit.Framework;
using Rounds2.UI;
using System;
using System.IO;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class DebugHudTextTests
    {
        [Test]
        public void FormatShowsServerAndClientState()
        {
            string text = DebugHudText.Format(serverStarted: true, clientStarted: false);

            Assert.AreEqual("Server: True\nClient: False", text);
        }

        [Test]
        public void ProjectSetupPlacesNetworkStatusAwayFromTopLeftLogo()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Editor", "Rounds2ProjectSetup.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("textRect.anchorMin = new Vector2(1f, 1f);", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("textRect.anchorMax = new Vector2(1f, 1f);", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("textRect.pivot = new Vector2(1f, 1f);", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("textRect.anchoredPosition = new Vector2(-16f, -16f);", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("statusText.alignment = TextAnchor.UpperRight;", StringComparison.Ordinal));
        }

        [Test]
        public void ArenaScenePlacesNetworkStatusAwayFromTopLeftLogo()
        {
            string scenePath = Path.Combine(Application.dataPath, "Scenes", "Arena01.unity");
            string scene = File.ReadAllText(scenePath);

            Assert.IsTrue(scene.Contains("m_AnchorMin: {x: 1, y: 1}", StringComparison.Ordinal));
            Assert.IsTrue(scene.Contains("m_AnchorMax: {x: 1, y: 1}", StringComparison.Ordinal));
            Assert.IsTrue(scene.Contains("m_AnchoredPosition: {x: -16, y: -16}", StringComparison.Ordinal));
            Assert.IsTrue(scene.Contains("m_Pivot: {x: 1, y: 1}", StringComparison.Ordinal));
            Assert.IsTrue(scene.Contains("m_Alignment: 2", StringComparison.Ordinal));
        }
    }
}
