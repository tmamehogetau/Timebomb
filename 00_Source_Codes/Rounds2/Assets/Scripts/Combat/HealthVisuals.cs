using UnityEngine;

namespace Rounds2.Combat
{
    public sealed class HealthVisuals : MonoBehaviour
    {
        private SpriteRenderer[] spriteRenderers;
        private Collider2D[] colliders;

        private void Awake()
        {
            CacheComponents();
        }

        public void SetAlive(bool alive)
        {
            CacheComponents();

            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = alive;
                }
            }

            foreach (Collider2D collider in colliders)
            {
                if (collider != null)
                {
                    collider.enabled = alive;
                }
            }
        }

        private void CacheComponents()
        {
            if (spriteRenderers == null || spriteRenderers.Length == 0)
            {
                spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            }

            colliders ??= GetComponents<Collider2D>();
        }
    }
}
