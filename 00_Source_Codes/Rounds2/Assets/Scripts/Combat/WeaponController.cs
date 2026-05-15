using FishNet.Object;
using Rounds2.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rounds2.Combat
{
    public sealed class WeaponController : NetworkBehaviour
    {
        [SerializeField] private Bullet bulletPrefab;
        [SerializeField] private Transform muzzle;

        private readonly WeaponFireGate fireGate = new();
        private PlayerController player;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
        }

        private void Update()
        {
            Mouse mouse = Mouse.current;
            if (!IsOwner || mouse == null || !mouse.leftButton.wasPressedThisFrame)
            {
                return;
            }

            FireServerRpc();
        }

        [ServerRpc]
        private void FireServerRpc()
        {
            if (bulletPrefab == null || muzzle == null || player == null || !fireGate.TryConsumeShot(Time.time))
            {
                return;
            }

            Bullet bullet = Instantiate(bulletPrefab, muzzle.position, Quaternion.identity);
            Spawn(bullet.gameObject);
            bullet.Launch(NetworkObject, player.AimDirection);
        }
    }
}
