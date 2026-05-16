using NUnit.Framework;
using System;
using System.IO;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class PlayerSeparationPrefabTests
    {
        [Test]
        public void PlayerPrefabDoesNotUsePositionPushingSeparationController()
        {
            string prefabPath = Path.Combine(Application.dataPath, "Prefabs", "Player.prefab");
            string prefabSource = File.ReadAllText(prefabPath);

            Assert.IsFalse(prefabSource.Contains("PlayerSeparationController", StringComparison.Ordinal));
            Assert.IsFalse(prefabSource.Contains("2e70a740445149d28e11e12f991b67d9", StringComparison.Ordinal));
        }

        [Test]
        public void PlayerControllerDoesNotOwnContactVelocityFiltering()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Player", "PlayerController.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsFalse(source.Contains("PlayerSeparationRules.FilterVelocity", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("ApplyCorrectionObserversRpc", StringComparison.Ordinal));
        }

        [Test]
        public void PlayerControllerDoesNotUseOwnerOnlyContactDistances()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Player", "PlayerController.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsFalse(source.Contains("ServerContactMinimumDistance", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("OwnerVisualContactMinimumDistance", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("isServer ? ServerContactMinimumDistance : OwnerVisualContactMinimumDistance", StringComparison.Ordinal));
        }

        [Test]
        public void PlayerControllerDoesNotToggleOwnerTransformDuringContact()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Player", "PlayerController.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsFalse(source.Contains("SetSendToOwner", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("OwnerTransformSyncEnableDistance", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("OwnerTransformSyncDisableDistance", StringComparison.Ordinal));
        }

        [Test]
        public void PlayerControllerDoesNotHoldLocalMovementWhenOwnerStartsMoving()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Player", "PlayerController.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsFalse(source.Contains("OwnerMoveStartHoldFixedTicks", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("ShouldHoldOwnerMovementStart", StringComparison.Ordinal));
        }
    }
}
