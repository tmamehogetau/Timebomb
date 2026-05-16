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
            Assert.IsTrue(bulletSource.Contains("if (!IsServerInitialized)", StringComparison.Ordinal));
        }

        [Test]
        public void BulletUsesCollisionAsPrimaryLifetime()
        {
            string bulletPath = Path.Combine(Application.dataPath, "Scripts", "Combat", "Bullet.cs");
            string bulletSource = File.ReadAllText(bulletPath);

            Assert.IsFalse(bulletSource.Contains("CombatTuning.BulletLifetimeSeconds", StringComparison.Ordinal));
            Assert.IsTrue(bulletSource.Contains("CombatTuning.BulletLeakSafetyLifetimeSeconds", StringComparison.Ordinal));
            Assert.IsTrue(bulletSource.Contains("GetComponentInParent<Health>()", StringComparison.Ordinal));
            Assert.IsTrue(bulletSource.Contains("if (!IsServerInitialized)", StringComparison.Ordinal));
            Assert.IsTrue(bulletSource.Contains("Despawn();", StringComparison.Ordinal));
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

        [Test]
        public void FireServerRpcConsumesAmmoOnlyAfterFireGateAllowsShot()
        {
            string weaponPath = Path.Combine(Application.dataPath, "Scripts", "Combat", "WeaponController.cs");
            string weaponSource = File.ReadAllText(weaponPath);

            Assert.IsTrue(weaponSource.Contains("WeaponAmmoState", StringComparison.Ordinal));
            Assert.IsTrue(weaponSource.Contains("fireGate.CanConsumeShot(currentTime)", StringComparison.Ordinal));
            Assert.IsTrue(weaponSource.Contains("ammo.TryConsumeShot(currentTime)", StringComparison.Ordinal));
            Assert.IsTrue(weaponSource.Contains("fireGate.ConsumeShot(currentTime)", StringComparison.Ordinal));
        }

        [Test]
        public void WeaponControllerPublishesAmmoChangesAndCanResetAmmo()
        {
            string weaponPath = Path.Combine(Application.dataPath, "Scripts", "Combat", "WeaponController.cs");
            string weaponSource = File.ReadAllText(weaponPath);

            Assert.IsTrue(weaponSource.Contains("AmmoChanged", StringComparison.Ordinal));
            Assert.IsTrue(weaponSource.Contains("CurrentAmmo", StringComparison.Ordinal));
            Assert.IsTrue(weaponSource.Contains("IsReloading", StringComparison.Ordinal));
            Assert.IsTrue(weaponSource.Contains("ResetAmmo", StringComparison.Ordinal));
        }

        [Test]
        public void SetManagerRefreshesHudWhenAmmoChanges()
        {
            string setManagerPath = Path.Combine(Application.dataPath, "Scripts", "Match", "SetManager.cs");
            string setManagerSource = File.ReadAllText(setManagerPath);

            Assert.IsTrue(setManagerSource.Contains("GetComponent<WeaponController>()", StringComparison.Ordinal));
            Assert.IsTrue(setManagerSource.Contains("weapon.AmmoChanged += OnWeaponAmmoChanged", StringComparison.Ordinal));
            Assert.IsTrue(setManagerSource.Contains("WeaponController weapon", StringComparison.Ordinal));
            Assert.IsTrue(setManagerSource.Contains("HealthHudText.Format", StringComparison.Ordinal));
        }

        [Test]
        public void ArenaHealthHudHasRoomForAmmoAndReloadingText()
        {
            string setupPath = Path.Combine(Application.dataPath, "Editor", "Rounds2ProjectSetup.cs");
            string scenePath = Path.Combine(Application.dataPath, "Scenes", "Arena01.unity");
            string setupSource = File.ReadAllText(setupPath);
            string sceneSource = File.ReadAllText(scenePath);

            Assert.IsTrue(setupSource.Contains("textRect.sizeDelta = new Vector2(760f, 40f)", StringComparison.Ordinal));
            Assert.IsTrue(sceneSource.Contains("m_SizeDelta: {x: 760, y: 40}", StringComparison.Ordinal));
        }
    }
}
