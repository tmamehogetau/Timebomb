using NUnit.Framework;
using Rounds2.Player;
using UnityEditor;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class PlayerContactCollisionTests
    {
        [Test]
        public void PlayerPrefabAllowsPredictedPhysicsContact()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");

            Assert.IsNull(prefab.GetComponent<PlayerContactCollision>());
        }
    }
}
