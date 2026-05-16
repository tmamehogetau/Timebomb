using NUnit.Framework;
using Rounds2.Development;
using Rounds2.Player;
using UnityEditor;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class PlayerBotPrefabTests
    {
        [Test]
        public void PlayerPrefabHasBotController()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");

            Assert.IsNotNull(prefab.GetComponent<PlayerBotController>());
        }

        [Test]
        public void PlayerPrefabHasDevelopmentShortcuts()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");

            Assert.IsNotNull(prefab.GetComponent<PlayerDevelopmentShortcuts>());
        }
    }
}
