using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class WeaponControllerLaunchOrderTests
    {
        [Test]
        public void FireServerRpcLaunchesBulletBeforeNetworkSpawn()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Combat", "WeaponController.cs");
            string source = File.ReadAllText(sourcePath);

            int launchIndex = source.IndexOf("bullet.Launch(NetworkObject, aim)", StringComparison.Ordinal);
            int spawnIndex = source.IndexOf("Spawn(bullet.gameObject)", StringComparison.Ordinal);

            Assert.GreaterOrEqual(launchIndex, 0);
            Assert.GreaterOrEqual(spawnIndex, 0);
            Assert.Less(launchIndex, spawnIndex);
        }

        [Test]
        public void BulletsDoNotApplyClientSidePhysicsVelocity()
        {
            string bulletPath = Path.Combine(Application.dataPath, "Scripts", "Combat", "Bullet.cs");
            string weaponPath = Path.Combine(Application.dataPath, "Scripts", "Combat", "WeaponController.cs");

            string bulletSource = File.ReadAllText(bulletPath);
            string weaponSource = File.ReadAllText(weaponPath);

            Assert.IsFalse(bulletSource.Contains("ApplyLaunchVelocityObserversRpc", StringComparison.Ordinal));
            Assert.IsFalse(weaponSource.Contains("ApplyLaunchVelocityObserversRpc", StringComparison.Ordinal));
        }

        [Test]
        public void BulletTriggerCallbackGuardsServerLogicWithoutServerAttributeWarning()
        {
            string bulletPath = Path.Combine(Application.dataPath, "Scripts", "Combat", "Bullet.cs");
            string bulletSource = File.ReadAllText(bulletPath);

            Assert.IsFalse(bulletSource.Contains("[Server]\r\n        private void OnTriggerEnter2D", StringComparison.Ordinal));
            Assert.IsTrue(bulletSource.Contains("if (!IsServer)", StringComparison.Ordinal));
        }

        [Test]
        public void FireServerRpcChecksRoundAndShooterState()
        {
            string weaponPath = Path.Combine(Application.dataPath, "Scripts", "Combat", "WeaponController.cs");
            string weaponSource = File.ReadAllText(weaponPath);

            Assert.IsTrue(weaponSource.Contains("WeaponFireRules.CanFire", StringComparison.Ordinal));
            Assert.IsTrue(weaponSource.Contains("RoundCombatGate.IsOpen", StringComparison.Ordinal));
            Assert.IsTrue(weaponSource.Contains("health.IsDead", StringComparison.Ordinal));
        }
    }
}
