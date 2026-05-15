using System;

namespace Rounds2.Combat
{
    public sealed class HealthState
    {
        public int Current { get; private set; }
        public bool IsDead => Current <= 0;

        public bool Reset(int baseHealth)
        {
            Current = Math.Max(0, baseHealth);
            return IsDead;
        }

        public bool ApplyDamage(int amount)
        {
            if (amount <= 0 || IsDead)
            {
                return false;
            }

            Current = Math.Max(0, Current - amount);
            return IsDead;
        }
    }
}
