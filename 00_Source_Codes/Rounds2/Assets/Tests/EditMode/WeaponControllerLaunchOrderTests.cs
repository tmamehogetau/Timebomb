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

            int launchIndex = source.IndexOf("bullet.Launch(NetworkObject, projectileAim, profile.DamageMultiplier, profile.SpeedMultiplier, profile.ProjectileBounces)", StringComparison.Ordinal);
            int spawnIndex = source.IndexOf("Spawn(bullet.gameObject)", StringComparison.Ordinal);

            Assert.GreaterOrEqual(launchIndex, 0);
            Assert.GreaterOrEqual(spawnIndex, 0);
            Assert.Less(launchIndex, spawnIndex);
        }

        [Test]
        public void WeaponControllerUsesCumulativeShotProfileForBurstAndSpread()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Combat", "WeaponController.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("ShotProfile profile", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("ammo.TryConsumeShots(currentTime, profile.AmmoCost, out int consumedShots)", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("StartCoroutine(FireBurst(profile, consumedShots", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("profile.ProjectileCount", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("profile.SpreadAngleDegrees", StringComparison.Ordinal));
        }

        [Test]
        public void WeaponControllerPassesRicochetBouncesToBullets()
        {
            string bulletPath = Path.Combine(Application.dataPath, "Scripts", "Combat", "Bullet.cs");
            string weaponPath = Path.Combine(Application.dataPath, "Scripts", "Combat", "WeaponController.cs");

            string bulletSource = File.ReadAllText(bulletPath);
            string weaponSource = File.ReadAllText(weaponPath);

            Assert.IsTrue(weaponSource.Contains("profile.ProjectileBounces", StringComparison.Ordinal));
            Assert.IsTrue(bulletSource.Contains("bouncesRemaining", StringComparison.Ordinal));
            Assert.IsTrue(bulletSource.Contains("Vector2.Reflect", StringComparison.Ordinal));
        }

        [Test]
        public void WeaponControllerFiresOnlyConsumedAmmoWhenMagazineCannotPayFullShotCost()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Combat", "WeaponController.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("private IEnumerator FireBurst(ShotProfile profile, int shotsToFire", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("int volleyShots = Mathf.Min(profile.ProjectileCount, remainingShots)", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("remainingShots -= volleyShots", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("FireVolley(profile, volleyShots", StringComparison.Ordinal));
        }

        [Test]
        public void FollowupBurstShotsRefreshSpawnAfterBurstDelay()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Combat", "WeaponController.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("Vector2 burstSpawn = i == 0", StringComparison.Ordinal));
            int refreshIndex = source.IndexOf("Vector2 burstSpawn = i == 0", StringComparison.Ordinal);
            int fireIndex = source.IndexOf("FireVolley(profile, volleyShots, burstSpawn, burstAim)", StringComparison.Ordinal);

            Assert.GreaterOrEqual(refreshIndex, 0);
            Assert.GreaterOrEqual(fireIndex, 0);
            Assert.Less(refreshIndex, fireIndex);
            Assert.IsFalse(source.Contains("FireVolley(profile, requestedSpawn, aim);\r\n                requestedSpawn =", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("FireVolley(profile, requestedSpawn, aim);\n                requestedSpawn =", StringComparison.Ordinal));
        }

        [Test]
        public void FollowupBurstShotsRefreshAimAfterBurstDelay()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Combat", "WeaponController.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("Vector2 burstAim = i == 0", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains(": player.AimDirection", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("WeaponSpawnPoint.FromShooter(transform.position, burstAim)", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("FireVolley(profile, volleyShots, burstSpawn, burstAim)", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("FireVolley(profile, volleyShots, burstSpawn, aim)", StringComparison.Ordinal));
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
        public void BulletsIgnoreOtherBulletsSoSplitShotsDoNotDespawnEachOther()
        {
            string bulletPath = Path.Combine(Application.dataPath, "Scripts", "Combat", "Bullet.cs");
            string bulletSource = File.ReadAllText(bulletPath);

            Assert.IsTrue(bulletSource.Contains("GetComponentInParent<Bullet>()", StringComparison.Ordinal));
        }

        [Test]
        public void WallImpactsLingerBrieflyBeforeBulletDespawns()
        {
            string bulletPath = Path.Combine(Application.dataPath, "Scripts", "Combat", "Bullet.cs");
            string bulletSource = File.ReadAllText(bulletPath);

            Assert.IsTrue(bulletSource.Contains("DespawnAfterWallImpact", StringComparison.Ordinal));
            Assert.IsTrue(bulletSource.Contains("body.linearVelocity = Vector2.zero", StringComparison.Ordinal));
            Assert.IsTrue(bulletSource.Contains("SetBulletCollidersEnabled(false)", StringComparison.Ordinal));
            Assert.IsTrue(bulletSource.Contains("CombatTuning.BulletWallImpactLingerSeconds", StringComparison.Ordinal));
        }

        [Test]
        public void FireServerRpcChecksRoundAndShooterState()
        {
            string weaponPath = Path.Combine(Application.dataPath, "Scripts", "Combat", "WeaponController.cs");
            string weaponSource = File.ReadAllText(weaponPath);

            Assert.IsTrue(weaponSource.Contains("WeaponFireRules.CanFire", StringComparison.Ordinal));
            Assert.IsTrue(weaponSource.Contains("RoundCombatGate.IsOpen", StringComparison.Ordinal));
            Assert.IsTrue(weaponSource.Contains("health.IsDead", StringComparison.Ordinal));
            Assert.IsTrue(weaponSource.Contains("shield.IsShielding", StringComparison.Ordinal));
        }

        [Test]
        public void FireServerRpcConsumesAmmoOnlyAfterFireGateAllowsShot()
        {
            string weaponPath = Path.Combine(Application.dataPath, "Scripts", "Combat", "WeaponController.cs");
            string weaponSource = File.ReadAllText(weaponPath);

            Assert.IsTrue(weaponSource.Contains("WeaponAmmoState", StringComparison.Ordinal));
            Assert.IsTrue(weaponSource.Contains("fireGate.CanConsumeShot(currentTime)", StringComparison.Ordinal));
            Assert.IsTrue(weaponSource.Contains("ammo.TryConsumeShots(currentTime, profile.AmmoCost, out int consumedShots)", StringComparison.Ordinal));
            Assert.IsTrue(weaponSource.Contains("fireGate.ConsumeShot(currentTime)", StringComparison.Ordinal));
        }

        [Test]
        public void WeaponControllerReplicatesSubtleFireFeedbackAfterAcceptedShot()
        {
            string weaponPath = Path.Combine(Application.dataPath, "Scripts", "Combat", "WeaponController.cs");
            string weaponSource = File.ReadAllText(weaponPath);

            Assert.IsTrue(weaponSource.Contains("PredictedPlayerMotor motor", StringComparison.Ordinal));
            Assert.IsTrue(weaponSource.Contains("[ObserversRpc(RunLocally = true)]", StringComparison.Ordinal));
            Assert.IsTrue(weaponSource.Contains("PlayFireFeedbackObserversRpc(aim)", StringComparison.Ordinal));
            Assert.IsTrue(weaponSource.Contains("motor.PlayFireFeedback(aim)", StringComparison.Ordinal));
        }

        [Test]
        public void WeaponControllerPublishesAmmoChangesAndCanResetAmmo()
        {
            string weaponPath = Path.Combine(Application.dataPath, "Scripts", "Combat", "WeaponController.cs");
            string weaponSource = File.ReadAllText(weaponPath);

            Assert.IsTrue(weaponSource.Contains("AmmoChanged", StringComparison.Ordinal));
            Assert.IsTrue(weaponSource.Contains("CurrentAmmo", StringComparison.Ordinal));
            Assert.IsTrue(weaponSource.Contains("IsReloading", StringComparison.Ordinal));
            Assert.IsTrue(weaponSource.Contains("ReloadRemaining", StringComparison.Ordinal));
            Assert.IsTrue(weaponSource.Contains("ResetAmmo", StringComparison.Ordinal));
        }

        [Test]
        public void SetManagerRefreshesHudWhenAmmoChanges()
        {
            string setManagerPath = Path.Combine(Application.dataPath, "Scripts", "Match", "SetManager.cs");
            string setManagerSource = File.ReadAllText(setManagerPath);

            Assert.IsTrue(setManagerSource.Contains("GetComponent<WeaponController>()", StringComparison.Ordinal));
            Assert.IsTrue(setManagerSource.Contains("weapon.AmmoChanged += OnWeaponAmmoChanged", StringComparison.Ordinal));
            Assert.IsTrue(setManagerSource.Contains("shield.ShieldChanged += OnShieldChanged", StringComparison.Ordinal));
            Assert.IsTrue(setManagerSource.Contains("WeaponController weapon", StringComparison.Ordinal));
            Assert.IsTrue(setManagerSource.Contains("HealthHudText.Format", StringComparison.Ordinal));
        }

        [Test]
        public void BulletHitConsumesShieldBeforeDamageAndKnockback()
        {
            string bulletPath = Path.Combine(Application.dataPath, "Scripts", "Combat", "Bullet.cs");
            string bulletSource = File.ReadAllText(bulletPath);

            int shieldIndex = bulletSource.IndexOf("TryConsumeShieldHit", StringComparison.Ordinal);
            int shieldFeedbackIndex = bulletSource.IndexOf("PlayShieldBlockFeedback", StringComparison.Ordinal);
            int knockbackIndex = bulletSource.IndexOf("ApplyKnockback(health)", StringComparison.Ordinal);
            int damageIndex = bulletSource.IndexOf("health.ApplyDamage", StringComparison.Ordinal);

            Assert.GreaterOrEqual(shieldIndex, 0);
            Assert.GreaterOrEqual(shieldFeedbackIndex, 0);
            Assert.GreaterOrEqual(knockbackIndex, 0);
            Assert.GreaterOrEqual(damageIndex, 0);
            Assert.Less(shieldIndex, shieldFeedbackIndex);
            Assert.Less(shieldFeedbackIndex, knockbackIndex);
            Assert.Less(shieldIndex, knockbackIndex);
            Assert.Less(shieldIndex, damageIndex);
        }

        [Test]
        public void HealthReplicatesSubtleDamageFeedbackToObservers()
        {
            string healthPath = Path.Combine(Application.dataPath, "Scripts", "Combat", "Health.cs");
            string healthSource = File.ReadAllText(healthPath);

            Assert.IsTrue(healthSource.Contains("PlayDamageFeedbackObserversRpc", StringComparison.Ordinal));
            Assert.IsTrue(healthSource.Contains("[ObserversRpc(RunLocally = true)]", StringComparison.Ordinal));
            Assert.IsTrue(healthSource.Contains("visuals?.PlayDamageFeedback()", StringComparison.Ordinal));
        }

        [Test]
        public void ShieldControllerReplicatesVisualStateToObservers()
        {
            string shieldPath = Path.Combine(Application.dataPath, "Scripts", "Player", "PlayerShieldController.cs");
            string shieldSource = File.ReadAllText(shieldPath);

            Assert.IsTrue(shieldSource.Contains("shieldVisual", StringComparison.Ordinal));
            Assert.IsTrue(shieldSource.Contains("CooldownRemaining", StringComparison.Ordinal));
            Assert.IsTrue(shieldSource.Contains("[ObserversRpc(BufferLast = true, RunLocally = true)]", StringComparison.Ordinal));
            Assert.IsTrue(shieldSource.Contains("SetShieldVisualObserversRpc(IsShielding)", StringComparison.Ordinal));
            Assert.IsTrue(shieldSource.Contains("shieldVisual.SetActive(active)", StringComparison.Ordinal));
            Assert.IsTrue(shieldSource.Contains("PlayShieldBlockFeedbackObserversRpc", StringComparison.Ordinal));
            Assert.IsTrue(shieldSource.Contains("CombatTuning.ShieldBlockFeedbackAlphaBoost", StringComparison.Ordinal));
        }

        [Test]
        public void ShieldBlockFeedbackUsesSubtleScalePulse()
        {
            string shieldPath = Path.Combine(Application.dataPath, "Scripts", "Player", "PlayerShieldController.cs");
            string shieldSource = File.ReadAllText(shieldPath);

            Assert.IsTrue(shieldSource.Contains("shieldBaseScale", StringComparison.Ordinal));
            Assert.IsTrue(shieldSource.Contains("ApplyShieldVisualScale", StringComparison.Ordinal));
            Assert.IsTrue(shieldSource.Contains("CombatTuning.ShieldBlockFeedbackScaleBoost", StringComparison.Ordinal));
            Assert.IsTrue(shieldSource.Contains("shieldVisual.transform.localScale", StringComparison.Ordinal));
        }

        [Test]
        public void PlayerStatusDisplayReplicatesAmmoAndCooldownTextToObservers()
        {
            string statusPath = Path.Combine(Application.dataPath, "Scripts", "UI", "PlayerStatusDisplay.cs");
            string statusSource = File.ReadAllText(statusPath);

            Assert.IsTrue(statusSource.Contains("PlayerStatusGaugeState.Create", StringComparison.Ordinal));
            Assert.IsTrue(statusSource.Contains("GaugeRootLocalPosition = new(0f, -0.24f, 0f)", StringComparison.Ordinal));
            Assert.IsTrue(statusSource.Contains("AmmoStackLocalPosition = new(-0.22f, 0f, 0f)", StringComparison.Ordinal));
            Assert.IsTrue(statusSource.Contains("ShieldPieLocalPosition = new(0.22f, 0f, 0f)", StringComparison.Ordinal));
            Assert.IsTrue(statusSource.Contains("AmmoStackRoot", StringComparison.Ordinal));
            Assert.IsTrue(statusSource.Contains("AmmoReloadCooldownFill", StringComparison.Ordinal));
            Assert.IsTrue(statusSource.Contains("ShieldCooldownPie", StringComparison.Ordinal));
            Assert.IsTrue(statusSource.Contains("weapon.ReloadRemaining", StringComparison.Ordinal));
            Assert.IsTrue(statusSource.Contains("shield.CooldownRemaining", StringComparison.Ordinal));
            Assert.IsTrue(statusSource.Contains("[ObserversRpc(BufferLast = true, RunLocally = true)]", StringComparison.Ordinal));
            Assert.IsTrue(statusSource.Contains("SetStatusGaugeObserversRpc", StringComparison.Ordinal));
            Assert.IsTrue(statusSource.Contains("SetAmmoSlots", StringComparison.Ordinal));
            Assert.IsTrue(statusSource.Contains("SetVerticalFill", StringComparison.Ordinal));
            Assert.IsTrue(statusSource.Contains("SetPieFill", StringComparison.Ordinal));
        }

        [Test]
        public void SetManagerStartsDraftForDefeatedPlayerBeforeRoundReset()
        {
            string setManagerPath = Path.Combine(Application.dataPath, "Scripts", "Match", "SetManager.cs");
            string setManagerSource = File.ReadAllText(setManagerPath);

            Assert.IsTrue(setManagerSource.Contains("PlayerCardLoadout", StringComparison.Ordinal));
            Assert.IsTrue(setManagerSource.Contains("CardDraftOffer.Create", StringComparison.Ordinal));
            Assert.IsTrue(setManagerSource.Contains("BeginDraft(deadIndex)", StringComparison.Ordinal));
            Assert.IsTrue(setManagerSource.Contains("SubmitDraftChoice", StringComparison.Ordinal));
            Assert.IsTrue(setManagerSource.Contains("MinimumDraftDisplaySeconds", StringComparison.Ordinal));
            Assert.IsTrue(setManagerSource.Contains("Time.time < draftChoiceUnlockTimeSeconds", StringComparison.Ordinal));
            Assert.IsTrue(setManagerSource.Contains("loadout.Grant", StringComparison.Ordinal));
            Assert.IsTrue(setManagerSource.Contains("resetRoundCoroutine = StartCoroutine(ResetRoundAfterDelay())", StringComparison.Ordinal));
            Assert.IsTrue(setManagerSource.Contains("CardText.NameWithEffect(reward)", StringComparison.Ordinal));
            Assert.IsFalse(setManagerSource.Contains("gained {reward}", StringComparison.Ordinal));
        }

        [Test]
        public void SetManagerShowsDraftOfferInDedicatedDraftHud()
        {
            string setManagerPath = Path.Combine(Application.dataPath, "Scripts", "Match", "SetManager.cs");
            string draftHudPath = Path.Combine(Application.dataPath, "Scripts", "UI", "DraftHud.cs");
            string setManagerSource = File.ReadAllText(setManagerPath);
            string draftHudSource = File.ReadAllText(draftHudPath);

            Assert.IsTrue(setManagerSource.Contains("SetDraftHudObserversRpc", StringComparison.Ordinal));
            Assert.IsTrue(setManagerSource.Contains("DraftHudText.FormatChoiceOffer", StringComparison.Ordinal));
            Assert.IsTrue(setManagerSource.Contains("DraftHud.SetDraftText(text)", StringComparison.Ordinal));
            Assert.IsTrue(draftHudSource.Contains("DraftHudRuntimeCanvas", StringComparison.Ordinal));
            Assert.IsTrue(draftHudSource.Contains("TextAnchor.MiddleCenter", StringComparison.Ordinal));
        }

        [Test]
        public void PlayerControllerSubmitsNumberKeyDraftChoicesToServer()
        {
            string controllerPath = Path.Combine(Application.dataPath, "Scripts", "Player", "PlayerController.cs");
            string controllerSource = File.ReadAllText(controllerPath);

            Assert.IsTrue(controllerSource.Contains("Keyboard.current", StringComparison.Ordinal));
            Assert.IsTrue(controllerSource.Contains("digit1Key.wasPressedThisFrame", StringComparison.Ordinal));
            Assert.IsTrue(controllerSource.Contains("SubmitDraftChoiceServerRpc", StringComparison.Ordinal));
            Assert.IsTrue(controllerSource.Contains("setManager.SubmitDraftChoice", StringComparison.Ordinal));
        }

        [Test]
        public void BotControllerAutoSelectsDraftChoiceForBotPlaytests()
        {
            string botPath = Path.Combine(Application.dataPath, "Scripts", "Player", "PlayerBotController.cs");
            string botSource = File.ReadAllText(botPath);

            Assert.IsTrue(botSource.Contains("TrySubmitDraftChoice", StringComparison.Ordinal));
            Assert.IsTrue(botSource.Contains("player.SubmitDraftChoice(0)", StringComparison.Ordinal));
        }

        [Test]
        public void WeaponAndShieldControllersReadCardLoadoutStatsOnRoundReset()
        {
            string weaponPath = Path.Combine(Application.dataPath, "Scripts", "Combat", "WeaponController.cs");
            string shieldPath = Path.Combine(Application.dataPath, "Scripts", "Player", "PlayerShieldController.cs");
            string statusPath = Path.Combine(Application.dataPath, "Scripts", "UI", "PlayerStatusDisplay.cs");
            string weaponSource = File.ReadAllText(weaponPath);
            string shieldSource = File.ReadAllText(shieldPath);
            string statusSource = File.ReadAllText(statusPath);

            Assert.IsTrue(weaponSource.Contains("PlayerCardLoadout", StringComparison.Ordinal));
            Assert.IsTrue(weaponSource.Contains("ammo.Reset(stats.MagazineSize, stats.ReloadSeconds)", StringComparison.Ordinal));
            Assert.IsTrue(weaponSource.Contains("public int MagazineSize => ammo.MagazineSize", StringComparison.Ordinal));
            Assert.IsTrue(weaponSource.Contains("public float ReloadSeconds => ammo.ReloadSeconds", StringComparison.Ordinal));
            Assert.IsTrue(shieldSource.Contains("PlayerCardLoadout", StringComparison.Ordinal));
            Assert.IsTrue(shieldSource.Contains("shield.Reset(stats.ShieldActiveSeconds, stats.ShieldCooldownSeconds)", StringComparison.Ordinal));
            Assert.IsTrue(statusSource.Contains("weapon != null ? weapon.MagazineSize : CombatTuning.MagazineSize", StringComparison.Ordinal));
            Assert.IsTrue(statusSource.Contains("weapon != null ? weapon.ReloadSeconds : CombatTuning.ReloadSeconds", StringComparison.Ordinal));
        }

        [Test]
        public void ArenaHealthHudHasRoomForAmmoAndReloadingText()
        {
            string setupPath = Path.Combine(Application.dataPath, "Editor", "Rounds2ProjectSetup.cs");
            string scenePath = Path.Combine(Application.dataPath, "Scenes", "Arena01.unity");
            string setupSource = File.ReadAllText(setupPath);
            string sceneSource = File.ReadAllText(scenePath);

            Assert.IsTrue(setupSource.Contains("textRect.sizeDelta = new Vector2(980f, 40f)", StringComparison.Ordinal));
            Assert.IsTrue(sceneSource.Contains("m_SizeDelta: {x: 980, y: 40}", StringComparison.Ordinal));
            Assert.IsTrue(setupSource.Contains("healthText.fontSize = 18", StringComparison.Ordinal));
            Assert.IsTrue(sceneSource.Contains("m_FontSize: 18", StringComparison.Ordinal));
        }

        [Test]
        public void ArenaScoreHudHasRoomForDraftChoices()
        {
            string setupPath = Path.Combine(Application.dataPath, "Editor", "Rounds2ProjectSetup.cs");
            string scenePath = Path.Combine(Application.dataPath, "Scenes", "Arena01.unity");
            string setupSource = File.ReadAllText(setupPath);
            string sceneSource = File.ReadAllText(scenePath);

            Assert.IsTrue(setupSource.Contains("textRect.sizeDelta = new Vector2(860f, 96f)", StringComparison.Ordinal));
            Assert.IsTrue(sceneSource.Contains("m_SizeDelta: {x: 860, y: 96}", StringComparison.Ordinal));
            Assert.IsTrue(setupSource.Contains("scoreText.fontSize = 20", StringComparison.Ordinal));
            Assert.IsTrue(sceneSource.Contains("m_FontSize: 20", StringComparison.Ordinal));
        }
    }
}
