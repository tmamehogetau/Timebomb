using FishNet.Object;
using Rounds2.Config;
using Rounds2.Development;
using Rounds2.Match;
using Rounds2.Player;
using System;
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
        private Health health;

        public event Action<WeaponController> AmmoChanged;

        public int CurrentAmmo => ammo.CurrentAmmo;
        public bool IsReloading => ammo.IsReloading;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            health = GetComponent<Health>();
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
                || !fireGate.CanConsumeShot(currentTime)
                || !ammo.TryConsumeShot(currentTime))
            {
                return;
            }

            fireGate.ConsumeShot(currentTime);
            NotifyAmmoChanged();

            Vector2 aim = requestedAim.sqrMagnitude > 0.001f ? requestedAim.normalized : player.AimDirection;
            Vector2 serverSpawn = WeaponSpawnPoint.FromShooter(transform.position, aim);
            Vector2 spawn = WeaponSpawnPoint.ResolveRequestedSpawn(serverSpawn, requestedSpawn, aim);

            Bullet bullet = Instantiate(bulletPrefab, spawn, Quaternion.identity);
            bullet.Launch(NetworkObject, aim);
            Spawn(bullet.gameObject);
        }

        [Server]
        public void ResetAmmo()
        {
            ammo.Reset();
            fireGate.Reset();
            NotifyAmmoChanged();
        }

        private void NotifyAmmoChanged()
        {
            AmmoChanged?.Invoke(this);
        }
    }
}
