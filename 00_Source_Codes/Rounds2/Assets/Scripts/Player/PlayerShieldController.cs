using FishNet.Object;
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
        private readonly PlayerShieldState shield = new(CombatTuning.ShieldActiveSeconds, CombatTuning.ShieldCooldownSeconds);
        private Health health;
        private string lastStatusLabel = "Ready";

        public event Action<PlayerShieldController> ShieldChanged;

        public bool IsShielding => shield.IsShielding(Time.time);
        public string StatusLabel => shield.StatusLabel(Time.time);

        private void Awake()
        {
            health = GetComponent<Health>();
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
        public void ResetShield()
        {
            shield.Reset();
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
            ShieldChanged?.Invoke(this);
        }
    }
}
