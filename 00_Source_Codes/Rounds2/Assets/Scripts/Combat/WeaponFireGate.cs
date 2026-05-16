using Rounds2.Config;

namespace Rounds2.Combat
{
    public sealed class WeaponFireGate
    {
        private float nextAllowedTime = float.NegativeInfinity;

        public bool CanConsumeShot(float currentTime)
        {
            return currentTime >= nextAllowedTime;
        }

        public void ConsumeShot(float currentTime)
        {
            nextAllowedTime = currentTime + CombatTuning.FireIntervalSeconds;
        }

        public bool TryConsumeShot(float currentTime)
        {
            if (!CanConsumeShot(currentTime))
            {
                return false;
            }

            ConsumeShot(currentTime);
            return true;
        }

        public void Reset()
        {
            nextAllowedTime = float.NegativeInfinity;
        }
    }
}
