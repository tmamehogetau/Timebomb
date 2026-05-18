using FishNet.Object;
using Rounds2.Cards;
using Rounds2.Config;
using Rounds2.Development;
using Rounds2.Match;
using Rounds2.Player;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rounds2.Combat
{
    [DisallowMultipleComponent]
    public sealed class WeaponController : NetworkBehaviour
    {
        [SerializeField] private Bullet bulletPrefab;
        [SerializeField] private Transform muzzle;

        private readonly WeaponFireGate fireGate = new();
        private readonly WeaponAmmoState ammo = new(CombatTuning.MagazineSize, CombatTuning.ReloadSeconds);
        private PlayerController player;
        private PredictedPlayerMotor motor;
        private PlayerShieldController shield;
        private Health health;
        private PlayerCardLoadout loadout;

        public event Action<WeaponController> AmmoChanged;

        public int CurrentAmmo => ammo.CurrentAmmo;
        public int MagazineSize => ammo.MagazineSize;
        public float ReloadSeconds => ammo.ReloadSeconds;
        public bool IsReloading => ammo.IsReloading;
        public float ReloadRemaining => ammo.ReloadRemaining(Time.time);

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            motor = GetComponent<PredictedPlayerMotor>();
            shield = GetComponent<PlayerShieldController>();
            health = GetComponent<Health>();
            loadout = GetComponent<PlayerCardLoadout>();
        }

        private void Update()
        {
            if (IsServerInitialized && ammo.UpdateReload(Time.time))
            {
                NotifyAmmoChanged();
            }

            Mouse mouse = Mouse.current;
            if (!IsOwner || DevelopmentRuntimeOptions.BotEnabled || mouse == null || !mouse.leftButton.wasPressedThisFrame)
            {
                return;
            }

            Vector2 aim = player != null ? player.AimDirection : Vector2.right;
            TryFire(aim);
        }

        public void TryFire(Vector2 aim)
        {
            if (!IsOwner)
            {
                return;
            }

            Vector2 requestedSpawn = WeaponSpawnPoint.FromShooter(transform.position, aim);
            FireServerRpc(requestedSpawn, aim);
        }

        [ServerRpc]
        private void FireServerRpc(Vector2 requestedSpawn, Vector2 requestedAim)
        {
            float currentTime = Time.time;
            if (bulletPrefab == null
                || muzzle == null
                || player == null
                || !WeaponFireRules.CanFire(RoundCombatGate.IsOpen, health != null && health.IsDead)
                || shield != null && shield.IsShielding
                || !fireGate.CanConsumeShot(currentTime))
            {
                return;
            }

            Vector2 aim = requestedAim.sqrMagnitude > 0.001f ? requestedAim.normalized : player.AimDirection;
            ShotProfile profile = CurrentShotProfile();
            if (!ammo.TryConsumeShots(currentTime, profile.AmmoCost, out int consumedShots))
            {
                return;
            }

            fireGate.ConsumeShot(currentTime);
            NotifyAmmoChanged();
            PlayFireFeedbackObserversRpc(aim);

            StartCoroutine(FireBurst(profile, consumedShots, requestedSpawn, aim));
        }

        [ObserversRpc(RunLocally = true)]
        private void PlayFireFeedbackObserversRpc(Vector2 aim)
        {
            motor ??= GetComponent<PredictedPlayerMotor>();
            if (motor != null)
            {
                motor.PlayFireFeedback(aim);
            }
        }

        private IEnumerator FireBurst(ShotProfile profile, int shotsToFire, Vector2 requestedSpawn, Vector2 aim)
        {
            int remainingShots = shotsToFire;
            for (int i = 0; i < profile.BurstCount && remainingShots > 0; i++)
            {
                if (!CanContinueBurst())
                {
                    yield break;
                }

                int volleyShots = Mathf.Min(profile.ProjectileCount, remainingShots);
                Vector2 burstAim = i == 0
                    ? aim
                    : player.AimDirection;
                Vector2 burstSpawn = i == 0
                    ? requestedSpawn
                    : WeaponSpawnPoint.FromShooter(transform.position, burstAim);
                FireVolley(profile, volleyShots, burstSpawn, burstAim);
                remainingShots -= volleyShots;

                if (i < profile.BurstCount - 1 && remainingShots > 0)
                {
                    yield return new WaitForSeconds(ShotProfile.BurstIntervalSeconds);
                }
            }
        }

        private bool CanContinueBurst()
        {
            return bulletPrefab != null
                && player != null
                && WeaponFireRules.CanFire(RoundCombatGate.IsOpen, health != null && health.IsDead)
                && !(shield != null && shield.IsShielding);
        }

        private void FireVolley(ShotProfile profile, int projectileCount, Vector2 requestedSpawn, Vector2 aim)
        {
            Vector2 baseAim = aim.sqrMagnitude > 0.001f ? aim.normalized : player.AimDirection;
            Vector2 serverSpawn = WeaponSpawnPoint.FromShooter(transform.position, baseAim);
            Vector2 spawn = WeaponSpawnPoint.ResolveRequestedSpawn(serverSpawn, requestedSpawn, baseAim);

            for (int i = 0; i < projectileCount; i++)
            {
                Vector2 projectileAim = SpreadAim(baseAim, projectileCount, profile.SpreadAngleDegrees, i);
                Bullet bullet = Instantiate(bulletPrefab, spawn, Quaternion.identity);
                bullet.Launch(NetworkObject, projectileAim, profile.DamageMultiplier, profile.SpeedMultiplier, profile.ProjectileBounces);
                Spawn(bullet.gameObject);
            }
        }

        private static Vector2 SpreadAim(Vector2 baseAim, int projectileCount, float spreadAngleDegrees, int projectileIndex)
        {
            if (projectileCount <= 1 || spreadAngleDegrees <= 0f)
            {
                return baseAim;
            }

            float angleStep = spreadAngleDegrees / (projectileCount - 1);
            float angle = -spreadAngleDegrees * 0.5f + angleStep * projectileIndex;
            return Quaternion.Euler(0f, 0f, angle) * baseAim;
        }

        private ShotProfile CurrentShotProfile()
        {
            loadout ??= GetComponent<PlayerCardLoadout>();
            return loadout != null ? loadout.Stats.ShotProfile : ShotProfile.Base;
        }

        [Server]
        public void ResetAmmo()
        {
            loadout ??= GetComponent<PlayerCardLoadout>();
            CombatCardStats stats = loadout != null ? loadout.Stats : CombatCardStats.Base;
            ammo.Reset(stats.MagazineSize, stats.ReloadSeconds);
            fireGate.Reset();
            NotifyAmmoChanged();
        }

        private void NotifyAmmoChanged()
        {
            AmmoChanged?.Invoke(this);
        }
    }
}
