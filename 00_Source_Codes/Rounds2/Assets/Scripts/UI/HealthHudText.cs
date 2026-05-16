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
    }
}
