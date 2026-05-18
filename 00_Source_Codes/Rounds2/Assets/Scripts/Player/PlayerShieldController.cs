using FishNet.Object;
using Rounds2.Cards;
using Rounds2.Combat;
using Rounds2.Config;
using Rounds2.Development;
using Rounds2.Match;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rounds2.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerShieldController : NetworkBehaviour
    {
        [SerializeField] private GameObject shieldVisual;

        private readonly PlayerShieldState shield = new(CombatTuning.ShieldActiveSeconds, CombatTuning.ShieldCooldownSeconds);
        private Health health;
        private PlayerCardLoadout loadout;
        private SpriteRenderer shieldVisualRenderer;
        private Color shieldBaseColor;
        private Vector3 shieldBaseScale;
        private bool hasShieldBaseColor;
        private bool hasShieldBaseScale;
        private float shieldBlockFeedbackUntilSeconds;
        private string lastStatusLabel = "Ready";

        public event Action<PlayerShieldController> ShieldChanged;

        public bool IsShielding => shield.IsShielding(Time.time);
        public float CooldownRemaining => shield.CooldownRemaining(Time.time);
        public string StatusLabel => shield.StatusLabel(Time.time);

        private void Awake()
        {
            health = GetComponent<Health>();
            loadout = GetComponent<PlayerCardLoadout>();
            shieldVisual ??= transform.Find("ShieldVisual")?.gameObject;
            shieldVisualRenderer = shieldVisual != null ? shieldVisual.GetComponent<SpriteRenderer>() : null;
            CaptureShieldBaseColor();
            CaptureShieldBaseScale();
            SetShieldVisual(false);
        }

        private void Update()
        {
            if (IsOwner
                && !DevelopmentRuntimeOptions.BotEnabled
                && Mouse.current != null
                && Mouse.current.rightButton.wasPressedThisFrame)
            {
                RequestShieldServerRpc();
            }

            if (IsServerInitialized)
            {
                NotifyIfStatusChanged();
            }

            UpdateShieldBlockFeedback();
        }

        [ServerRpc]
        private void RequestShieldServerRpc()
        {
            TryActivateShield();
        }

        [Server]
        public bool TryActivateShield()
        {
            if (!RoundCombatGate.IsOpen || health != null && health.IsDead)
            {
                return false;
            }

            if (!shield.TryActivate(Time.time))
            {
                return false;
            }

            NotifyShieldChanged();
            return true;
        }

        [Server]
        public bool TryConsumeShieldHit()
        {
            if (!IsShielding)
            {
                return false;
            }

            NotifyShieldChanged();
            return true;
        }

        [Server]
        public void PlayShieldBlockFeedback()
        {
            if (IsSpawned)
            {
                PlayShieldBlockFeedbackObserversRpc();
                return;
            }

            PlayShieldBlockFeedbackLocal();
        }

        [Server]
        public void ResetShield()
        {
            loadout ??= GetComponent<PlayerCardLoadout>();
            CombatCardStats stats = loadout != null ? loadout.Stats : CombatCardStats.Base;
            shield.Reset(stats.ShieldActiveSeconds, stats.ShieldCooldownSeconds);
            NotifyShieldChanged();
        }

        private void NotifyIfStatusChanged()
        {
            string current = StatusLabel;
            if (current == lastStatusLabel)
            {
                return;
            }

            NotifyShieldChanged();
        }

        private void NotifyShieldChanged()
        {
            lastStatusLabel = StatusLabel;
            SetShieldVisualObserversRpc(IsShielding);
            ShieldChanged?.Invoke(this);
        }

        [ObserversRpc(BufferLast = true, RunLocally = true)]
        private void SetShieldVisualObserversRpc(bool active)
        {
            SetShieldVisual(active);
        }

        private void SetShieldVisual(bool active)
        {
            if (shieldVisual != null)
            {
                shieldVisual.SetActive(active);
            }

            ApplyShieldVisualColor();
            ApplyShieldVisualScale();
        }

        [ObserversRpc(RunLocally = true)]
        private void PlayShieldBlockFeedbackObserversRpc()
        {
            PlayShieldBlockFeedbackLocal();
        }

        private void PlayShieldBlockFeedbackLocal()
        {
            shieldBlockFeedbackUntilSeconds = Time.time + CombatTuning.ShieldBlockFeedbackSeconds;
            SetShieldVisual(true);
        }

        private void UpdateShieldBlockFeedback()
        {
            if (Time.time >= shieldBlockFeedbackUntilSeconds)
            {
                ApplyShieldVisualColor();
                ApplyShieldVisualScale();
                return;
            }

            ApplyShieldVisualColor();
            ApplyShieldVisualScale();
        }

        private void CaptureShieldBaseColor()
        {
            if (hasShieldBaseColor || shieldVisualRenderer == null)
            {
                return;
            }

            shieldBaseColor = shieldVisualRenderer.color;
            hasShieldBaseColor = true;
        }

        private void CaptureShieldBaseScale()
        {
            if (hasShieldBaseScale || shieldVisual == null)
            {
                return;
            }

            shieldBaseScale = shieldVisual.transform.localScale;
            hasShieldBaseScale = true;
        }

        private void ApplyShieldVisualColor()
        {
            if (shieldVisualRenderer == null)
            {
                return;
            }

            CaptureShieldBaseColor();
            Color color = shieldBaseColor;
            if (Time.time < shieldBlockFeedbackUntilSeconds)
            {
                float remaining = Mathf.Clamp01((shieldBlockFeedbackUntilSeconds - Time.time) / CombatTuning.ShieldBlockFeedbackSeconds);
                color.a = Mathf.Clamp01(shieldBaseColor.a + CombatTuning.ShieldBlockFeedbackAlphaBoost * remaining);
            }

            shieldVisualRenderer.color = color;
        }

        private void ApplyShieldVisualScale()
        {
            if (shieldVisual == null)
            {
                return;
            }

            CaptureShieldBaseScale();
            Vector3 scale = shieldBaseScale;
            if (Time.time < shieldBlockFeedbackUntilSeconds)
            {
                float remaining = Mathf.Clamp01((shieldBlockFeedbackUntilSeconds - Time.time) / CombatTuning.ShieldBlockFeedbackSeconds);
                scale = shieldBaseScale * (1f + CombatTuning.ShieldBlockFeedbackScaleBoost * remaining);
            }

            shieldVisual.transform.localScale = scale;
        }
    }
}
