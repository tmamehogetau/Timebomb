using FishNet.Component.Transforming;
using FishNet.Object;
using NUnit.Framework;
using Rounds2.Player;
using UnityEditor;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class PredictedPlayerPrefabTests
    {
        [Test]
        public void PlayerPrefabUsesPredictedMovementComponents()
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");

            Assert.IsNotNull(playerPrefab.GetComponent<PredictedPlayerMotor>());
            Assert.IsNotNull(playerPrefab.GetComponent<PlayerInputReader>());
        }

        [Test]
        public void PlayerPrefabDoesNotUseNetworkTransformForMovement()
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");

            Assert.IsNull(playerPrefab.GetComponent<NetworkTransform>());
        }

        [Test]
        public void PlayerPrefabEnablesRigidbody2DPredictionAndStateForwarding()
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
            NetworkObject networkObject = playerPrefab.GetComponent<NetworkObject>();
            SerializedObject serializedObject = new(networkObject);

            Assert.IsTrue(serializedObject.FindProperty("_enablePrediction").boolValue);
            Assert.AreEqual(2, serializedObject.FindProperty("_predictionType").intValue);
            Assert.IsTrue(serializedObject.FindProperty("_enableStateForwarding").boolValue);
        }
    }
}
