using System;

namespace Rounds2.UI
{
    public static class HealthHudText
    {
        public static string Format(int leftHealth, int rightHealth, int maxHealth)
        {
            int left = Math.Max(0, leftHealth);
            int right = Math.Max(0, rightHealth);
            return $"P1 HP {left}/{maxHealth}    P2 HP {right}/{maxHealth}";
        }

        public static string Format(
            int leftHealth,
            int rightHealth,
            int maxHealth,
            int leftAmmo,
            int rightAmmo,
            int magazineSize,
            bool leftReloading,
            bool rightReloading)
        {
            return Format(
                leftHealth,
                rightHealth,
                maxHealth,
                leftAmmo,
                rightAmmo,
                magazineSize,
                leftReloading,
                rightReloading,
                "Ready",
                "Ready");
        }

        public static string Format(
            int leftHealth,
            int rightHealth,
            int maxHealth,
            int leftAmmo,
            int rightAmmo,
            int magazineSize,
            bool leftReloading,
            bool rightReloading,
            string leftShield,
            string rightShield)
        {
            int left = Math.Max(0, leftHealth);
            int right = Math.Max(0, rightHealth);
            string leftReload = leftReloading ? " Reloading" : string.Empty;
            string rightReload = rightReloading ? " Reloading" : string.Empty;
            return $"P1 HP {left}/{maxHealth} Ammo {leftAmmo}/{magazineSize}{leftReload} Shield {leftShield}    P2 HP {right}/{maxHealth} Ammo {rightAmmo}/{magazineSize}{rightReload} Shield {rightShield}";
        }
    }
}
