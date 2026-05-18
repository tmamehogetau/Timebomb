using Rounds2.Config;
using UnityEngine;

namespace Rounds2.Combat
{
    public sealed class HealthVisuals : MonoBehaviour
    {
        private SpriteRenderer[] spriteRenderers;
        private Collider2D[] colliders;
        private SpriteRenderer bodyRenderer;
        private Color bodyBaseColor;
        private bool hasBodyBaseColor;
        private bool damageFeedbackActive;
        private float damageFeedbackUntilSeconds;

        private void Awake()
        {
            CacheComponents();
        }

        private void Update()
        {
            UpdateDamageFeedback();
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

        public void PlayDamageFeedback()
        {
            CacheComponents();
            if (bodyRenderer == null)
            {
                return;
            }

            CaptureBodyBaseColor();
            damageFeedbackActive = true;
            damageFeedbackUntilSeconds = Time.time + CombatTuning.DamageFeedbackSeconds;
            bodyRenderer.color = Color.Lerp(bodyBaseColor, Color.white, CombatTuning.DamageFeedbackWhiteBlend);
        }

        private void CacheComponents()
        {
            bodyRenderer ??= GetComponent<SpriteRenderer>();
            if (spriteRenderers == null || spriteRenderers.Length == 0)
            {
                spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            }

            colliders ??= GetComponents<Collider2D>();
        }

        private void CaptureBodyBaseColor()
        {
            if (hasBodyBaseColor || bodyRenderer == null)
            {
                return;
            }

            bodyBaseColor = bodyRenderer.color;
            hasBodyBaseColor = true;
        }

        private void UpdateDamageFeedback()
        {
            if (!damageFeedbackActive || bodyRenderer == null || !hasBodyBaseColor)
            {
                return;
            }

            if (Time.time >= damageFeedbackUntilSeconds)
            {
                bodyRenderer.color = bodyBaseColor;
                damageFeedbackActive = false;
                return;
            }

            float remaining = Mathf.Clamp01((damageFeedbackUntilSeconds - Time.time) / CombatTuning.DamageFeedbackSeconds);
            bodyRenderer.color = Color.Lerp(bodyBaseColor, Color.white, CombatTuning.DamageFeedbackWhiteBlend * remaining);
        }
    }
}
