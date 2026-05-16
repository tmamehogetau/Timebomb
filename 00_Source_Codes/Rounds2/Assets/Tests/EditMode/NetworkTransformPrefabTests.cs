using FishNet.Component.Transforming;
using FishNet.Object;
using NUnit.Framework;
using Rounds2.Config;
using UnityEditor;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class NetworkTransformPrefabTests
    {
        [Test]
        public void PlayerPrefabDoesNotUseNetworkTransformForPredictedMovement()
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
            NetworkTransform networkTransform = playerPrefab.GetComponent<NetworkTransform>();

            Assert.IsNull(networkTransform);
        }

        [Test]
        public void BulletPrefabStillSendsServerTransformToOwner()
        {
            GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Bullet.prefab");
            NetworkTransform networkTransform = bulletPrefab.GetComponent<NetworkTransform>();

            Assert.IsNotNull(networkTransform);
            Assert.IsTrue(networkTransform.GetSendToOwner());
        }

        [TestCase("Assets/Prefabs/Bullet.prefab")]
        public void NetworkedActionPrefabsUseLowLatencyTransformSettings(string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            NetworkTransform networkTransform = prefab.GetComponent<NetworkTransform>();
            SerializedObject serializedTransform = new(networkTransform);

            Assert.AreEqual(NetworkTuning.TransformSendIntervalTicks, serializedTransform.FindProperty("_interval").intValue);
            Assert.AreEqual(NetworkTuning.TransformInterpolationTicks, serializedTransform.FindProperty("_interpolation").intValue);
            Assert.AreEqual(NetworkTuning.TransformExtrapolationTicks, serializedTransform.FindProperty("_extrapolation").intValue);
        }

        [TestCase("Assets/Prefabs/Bullet.prefab")]
        public void NetworkedActionPrefabsUseLowLatencySpectatorSmoothing(string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            NetworkObject networkObject = prefab.GetComponent<NetworkObject>();
            SerializedObject serializedObject = new(networkObject);

            Assert.AreEqual((int)AdaptiveInterpolationType.Off, serializedObject.FindProperty("_adaptiveInterpolation").enumValueIndex);
            Assert.AreEqual(NetworkTuning.OwnerInterpolationTicks, serializedObject.FindProperty("_ownerInterpolation").intValue);
            Assert.AreEqual(NetworkTuning.SpectatorInterpolationTicks, serializedObject.FindProperty("_spectatorInterpolation").intValue);
        }

        [Test]
        public void NetworkTuningUsesActionGameTickRate()
        {
            Assert.AreEqual(60, NetworkTuning.TickRate);
            Assert.AreEqual(1f / 60f, NetworkTuning.FixedDeltaTime);
        }
    }
}
