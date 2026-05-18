using UnityEngine;

namespace Rounds2.UI
{
    public readonly struct PlayerStatusGaugeState
    {
        public PlayerStatusGaugeState(int activeAmmoSlots, int magazineSize, float reloadFill, float shieldCooldownFill)
        {
            ActiveAmmoSlots = activeAmmoSlots;
            MagazineSize = magazineSize;
            ReloadFill = reloadFill;
            ShieldCooldownFill = shieldCooldownFill;
        }

        public int ActiveAmmoSlots { get; }
        public int MagazineSize { get; }
        public float ReloadFill { get; }
        public float ShieldCooldownFill { get; }

        public static PlayerStatusGaugeState Create(
            int currentAmmo,
            int magazineSize,
            bool weaponReloading,
            float weaponReloadRemaining,
            float weaponReloadSeconds,
            float shieldCooldownRemaining,
            float shieldCooldownSeconds)
        {
            int slots = Mathf.Max(0, magazineSize);
            int activeSlots = Mathf.Clamp(currentAmmo, 0, slots);
            float reloadFill = weaponReloading ? FillFromRemaining(weaponReloadRemaining, weaponReloadSeconds) : 0f;
            float shieldFill = FillFromRemaining(shieldCooldownRemaining, shieldCooldownSeconds);
            return new PlayerStatusGaugeState(activeSlots, slots, reloadFill, shieldFill);
        }

        private static float FillFromRemaining(float remaining, float total)
        {
            if (total <= 0f)
            {
                return 1f;
            }

            return Mathf.Clamp01(1f - Mathf.Max(0f, remaining) / total);
        }
    }
}
