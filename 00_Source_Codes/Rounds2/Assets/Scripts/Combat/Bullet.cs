using FishNet.Object;
using Rounds2.Config;
using UnityEngine;

namespace Rounds2.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Bullet : NetworkBehaviour
    {
        private Rigidbody2D body;
        private Collider2D[] colliders;
        private NetworkObject owner;

        private void Awake()
        {
            CacheComponents();
        }

        public void Launch(NetworkObject firingOwner, Vector2 direction)
        {
            CacheComponents();
            owner = firingOwner;
            IgnoreOwnerCollisions();
            ApplyLaunchVelocity(direction);
        }

        private void ApplyLaunchVelocity(Vector2 direction)
        {
            Vector2 launchDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.right;
            body.linearVelocity = launchDirection * CombatTuning.BulletSpeed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsServer)
            {
                return;
            }

            if (owner != null && other.GetComponentInParent<NetworkObject>() == owner)
            {
                return;
            }

            if (other.GetComponentInParent<Health>() is Health health)
            {
                string ownerName = owner != null ? owner.name : "unknown";
                Debug.Log($"Rounds2 hit shooter={ownerName} target={health.name} damage={CombatTuning.BulletDamage} bulletPos={FormatPosition(transform.position)} targetPos={FormatPosition(health.transform.position)}");
                health.ApplyDamage(CombatTuning.BulletDamage);
                if (IsSpawned)
                {
                    Despawn();
                }

                return;
            }

            if (IsSpawned)
            {
                Despawn();
            }
        }

        private void CacheComponents()
        {
            body ??= gameObject.GetComponent<Rigidbody2D>();
            colliders ??= gameObject.GetComponents<Collider2D>();
        }

        private void IgnoreOwnerCollisions()
        {
            BulletOwnerCollision.Ignore(colliders, owner);
        }

        private static string FormatPosition(Vector3 position)
        {
            return $"({position.x:0.00},{position.y:0.00})";
        }
    }
}
