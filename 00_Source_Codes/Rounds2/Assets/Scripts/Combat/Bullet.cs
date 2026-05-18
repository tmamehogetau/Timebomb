using FishNet.Object;
using Rounds2.Config;
using Rounds2.Player;
using System.Collections;
using UnityEngine;

namespace Rounds2.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Bullet : NetworkBehaviour
    {
        private Rigidbody2D body;
        private Collider2D[] colliders;
        private TrailRenderer trailRenderer;
        private NetworkObject owner;
        private float launchedAtSeconds;
        private float damageMultiplier = 1f;
        private float speedMultiplier = 1f;
        private int bouncesRemaining;
        private bool impactDespawnPending;
        private bool canHitOwnerAfterRicochet;

        private void Awake()
        {
            CacheComponents();
            ConfigureTrailRenderer(emitting: false);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            StartCoroutine(EnableTrailAfterSpawnFrame());
        }

        private void OnDisable()
        {
            if (trailRenderer == null)
            {
                return;
            }

            trailRenderer.emitting = false;
            trailRenderer.Clear();
        }

        public void Launch(NetworkObject firingOwner, Vector2 direction)
        {
            Launch(firingOwner, direction, damageMultiplier: 1f, speedMultiplier: 1f);
        }

        public void Launch(NetworkObject firingOwner, Vector2 direction, float damageMultiplier, float speedMultiplier)
        {
            Launch(firingOwner, direction, damageMultiplier, speedMultiplier, projectileBounces: 0);
        }

        public void Launch(NetworkObject firingOwner, Vector2 direction, float damageMultiplier, float speedMultiplier, int projectileBounces)
        {
            CacheComponents();
            ConfigureTrailRenderer(emitting: false);
            owner = firingOwner;
            launchedAtSeconds = Time.time;
            this.damageMultiplier = Mathf.Max(0.05f, damageMultiplier);
            this.speedMultiplier = Mathf.Max(0.1f, speedMultiplier);
            bouncesRemaining = Mathf.Max(0, projectileBounces);
            impactDespawnPending = false;
            canHitOwnerAfterRicochet = false;
            SetBulletCollidersEnabled(true);
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
            body.linearVelocity = launchDirection * CombatTuning.BulletSpeed * speedMultiplier;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsServerInitialized)
            {
                return;
            }

            if (!canHitOwnerAfterRicochet && owner != null && other.GetComponentInParent<NetworkObject>() == owner)
            {
                return;
            }

            if (other.GetComponentInParent<Bullet>() != null)
            {
                return;
            }

            if (other.GetComponentInParent<Health>() is Health health)
            {
                string ownerName = owner != null ? owner.name : "unknown";
                int damage = Mathf.Max(1, Mathf.RoundToInt(CombatTuning.BulletDamage * damageMultiplier));
                Debug.Log($"Rounds2 hit shooter={ownerName} target={health.name} damage={damage} bulletPos={FormatPosition(transform.position)} targetPos={FormatPosition(health.transform.position)}");
                if (health.GetComponent<PlayerShieldController>() is PlayerShieldController shield && shield.TryConsumeShieldHit())
                {
                    shield.PlayShieldBlockFeedback();
                    if (IsSpawned)
                    {
                        Despawn();
                    }

                    return;
                }

                ApplyKnockback(health);
                health.ApplyDamage(damage);
                if (IsSpawned)
                {
                    Despawn();
                }

                return;
            }

            if (TryRicochet(other))
            {
                return;
            }

            StartCoroutine(DespawnAfterWallImpact());
        }

        private bool TryRicochet(Collider2D other)
        {
            if (bouncesRemaining <= 0 || body == null)
            {
                return false;
            }

            Vector2 velocity = body.linearVelocity;
            if (velocity.sqrMagnitude <= 0.001f)
            {
                return false;
            }

            Vector2 closestPoint = other.ClosestPoint(body.position);
            Vector2 normal = body.position - closestPoint;
            if (normal.sqrMagnitude <= 0.0001f)
            {
                normal = Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y)
                    ? new Vector2(-Mathf.Sign(velocity.x), 0f)
                    : new Vector2(0f, -Mathf.Sign(velocity.y));
            }

            body.linearVelocity = Vector2.Reflect(velocity, normal.normalized);
            bouncesRemaining--;
            canHitOwnerAfterRicochet = true;
            RestoreOwnerCollisions();
            return true;
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
            trailRenderer ??= gameObject.GetComponent<TrailRenderer>();
        }

        private IEnumerator DespawnAfterWallImpact()
        {
            if (impactDespawnPending)
            {
                yield break;
            }

            impactDespawnPending = true;
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }

            SetBulletCollidersEnabled(false);
            yield return new WaitForSeconds(CombatTuning.BulletWallImpactLingerSeconds);

            if (IsSpawned)
            {
                Despawn();
            }
        }

        private void SetBulletCollidersEnabled(bool enabled)
        {
            CacheComponents();
            if (colliders == null)
            {
                return;
            }

            foreach (Collider2D bulletCollider in colliders)
            {
                if (bulletCollider != null)
                {
                    bulletCollider.enabled = enabled;
                }
            }
        }

        private IEnumerator EnableTrailAfterSpawnFrame()
        {
            ConfigureTrailRenderer(emitting: false);
            yield return null;

            if (trailRenderer == null)
            {
                yield break;
            }

            trailRenderer.Clear();
            trailRenderer.emitting = true;
        }

        private void ConfigureTrailRenderer(bool emitting)
        {
            if (trailRenderer == null)
            {
                trailRenderer = gameObject.AddComponent<TrailRenderer>();
            }

            trailRenderer.time = CombatTuning.BulletTrailSeconds;
            trailRenderer.startWidth = CombatTuning.BulletTrailStartWidth;
            trailRenderer.endWidth = CombatTuning.BulletTrailEndWidth;
            trailRenderer.startColor = new Color(1f, 0.95f, 0.56f, 0.42f);
            trailRenderer.endColor = new Color(1f, 0.95f, 0.56f, 0f);
            trailRenderer.numCapVertices = 2;
            trailRenderer.alignment = LineAlignment.View;
            trailRenderer.textureMode = LineTextureMode.Stretch;
            trailRenderer.sortingOrder = 3;
            trailRenderer.emitting = emitting;
            trailRenderer.Clear();
        }

        private void IgnoreOwnerCollisions()
        {
            BulletOwnerCollision.Ignore(colliders, owner);
        }

        private void RestoreOwnerCollisions()
        {
            BulletOwnerCollision.Restore(colliders, owner);
        }

        private static string FormatPosition(Vector3 position)
        {
            return $"({position.x:0.00},{position.y:0.00})";
        }
    }
}
