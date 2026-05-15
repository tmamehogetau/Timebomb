using Rounds2.Config;

namespace Rounds2.Combat
{
    public sealed class WeaponFireGate
    {
        private float nextAllowedTime = float.NegativeInfinity;

        public bool TryConsumeShot(float currentTime)
        {
            if (currentTime < nextAllowedTime)
            {
                return false;
            }

            nextAllowedTime = currentTime + CombatTuning.FireIntervalSeconds;
            return true;
        }
    }
}
