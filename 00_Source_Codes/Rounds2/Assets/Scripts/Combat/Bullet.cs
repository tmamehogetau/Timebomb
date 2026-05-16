using FishNet.Object;
using Rounds2.Config;
using Rounds2.Player;
using UnityEngine;

namespace Rounds2.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Bullet : NetworkBehaviour
    {
        private Rigidbody2D body;
        private Collider2D[] colliders;
        private NetworkObject owner;
        private float launchedAtSeconds;

        private void Awake()
        {
            CacheComponents();
        }

        public void Launch(NetworkObject firingOwner, Vector2 direction)
        {
            CacheComponents();
            owner = firingOwner;
            launchedAtSeconds = Time.time;
            IgnoreOwnerCollisions();
            ApplyLaunchVelocity(direction);
        }

        private void Update()
        {
            if (!IsServerInitialized)
            {
                return;
            }

            if (Time.time - launchedAtSeconds >= CombatTuning.BulletLeakSafetyLifetimeSeconds && IsSpawned)
            {
                Despawn();
            }
        }

        private void ApplyLaunchVelocity(Vector2 direction)
        {
            Vector2 launchDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.right;
            body.linearVelocity = launchDirection * CombatTuning.BulletSpeed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsServerInitialized)
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
                ApplyKnockback(health);
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

        private void ApplyKnockback(Health target)
        {
            if (target.GetComponent<PredictedPlayerMotor>() is not PredictedPlayerMotor motor)
            {
                return;
            }

            Vector2 direction = body.linearVelocity.sqrMagnitude > 0.001f
                ? body.linearVelocity.normalized
                : (target.transform.position - transform.position).normalized;

            motor.ApplyExternalForce(
                direction,
                CombatTuning.BulletKnockbackSpeed,
                CombatTuning.BulletKnockbackDurationTicks);
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
