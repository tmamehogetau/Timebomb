using System;
using UnityEngine;

namespace Rounds2.Player
{
    public sealed class PlayerShieldState
    {
        private float activeSeconds;
        private float cooldownSeconds;
        private float shieldEndsAt = float.NegativeInfinity;
        private float cooldownEndsAt = float.NegativeInfinity;

        public PlayerShieldState(float activeSeconds, float cooldownSeconds)
        {
            Configure(activeSeconds, cooldownSeconds);
        }

        private void Configure(float activeSeconds, float cooldownSeconds)
        {
            if (activeSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(activeSeconds));
            }

            if (cooldownSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(cooldownSeconds));
            }

            this.activeSeconds = activeSeconds;
            this.cooldownSeconds = cooldownSeconds;
        }

        public bool TryActivate(float currentTime)
        {
            if (!IsReady(currentTime))
            {
                return false;
            }

            shieldEndsAt = currentTime + activeSeconds;
            cooldownEndsAt = currentTime + cooldownSeconds;
            return true;
        }

        public bool IsShielding(float currentTime)
        {
            return currentTime < shieldEndsAt;
        }

        public bool IsReady(float currentTime)
        {
            return currentTime >= cooldownEndsAt;
        }

        public float CooldownRemaining(float currentTime)
        {
            return Mathf.Max(0f, cooldownEndsAt - currentTime);
        }

        public string StatusLabel(float currentTime)
        {
            if (IsShielding(currentTime))
            {
                return "Shielding";
            }

            float remaining = CooldownRemaining(currentTime);
            return remaining > 0f ? $"{Mathf.CeilToInt(remaining)}s" : "Ready";
        }

        public void Reset()
        {
            shieldEndsAt = float.NegativeInfinity;
            cooldownEndsAt = float.NegativeInfinity;
        }

        public void Reset(float activeSeconds, float cooldownSeconds)
        {
            Configure(activeSeconds, cooldownSeconds);
            Reset();
        }
    }
}
