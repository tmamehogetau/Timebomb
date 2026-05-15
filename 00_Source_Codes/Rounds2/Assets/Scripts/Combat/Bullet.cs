using FishNet.Object;
using Rounds2.Config;
using UnityEngine;

namespace Rounds2.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Bullet : NetworkBehaviour
    {
        private Rigidbody2D body;
        private NetworkObject owner;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        [Server]
        public void Launch(NetworkObject firingOwner, Vector2 direction)
        {
            owner = firingOwner;
            body.linearVelocity = direction.normalized * CombatTuning.BulletSpeed;
        }

        [Server]
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (owner != null && other.TryGetComponent(out NetworkObject hitObject) && hitObject == owner)
            {
                return;
            }

            if (other.TryGetComponent(out Health health))
            {
                health.ApplyDamage(CombatTuning.BulletDamage);
                Despawn();
                return;
            }

            Despawn();
        }
    }
}
