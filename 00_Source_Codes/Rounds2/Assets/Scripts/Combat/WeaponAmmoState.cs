using System;

namespace Rounds2.Combat
{
    public sealed class WeaponAmmoState
    {
        private readonly int magazineSize;
        private readonly float reloadSeconds;
        private float reloadCompleteTime = float.PositiveInfinity;

        public WeaponAmmoState(int magazineSize, float reloadSeconds)
        {
            if (magazineSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(magazineSize));
            }

            if (reloadSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(reloadSeconds));
            }

            this.magazineSize = magazineSize;
            this.reloadSeconds = reloadSeconds;
            CurrentAmmo = magazineSize;
        }

        public int CurrentAmmo { get; private set; }
        public bool IsReloading => !float.IsPositiveInfinity(reloadCompleteTime);

        public bool UpdateReload(float currentTime)
        {
            if (!IsReloading || currentTime < reloadCompleteTime)
            {
                return false;
            }

            CurrentAmmo = magazineSize;
            reloadCompleteTime = float.PositiveInfinity;
            return true;
        }

        public bool TryConsumeShot(float currentTime)
        {
            UpdateReload(currentTime);

            if (IsReloading || CurrentAmmo <= 0)
            {
                return false;
            }

            CurrentAmmo--;
            if (CurrentAmmo == 0)
            {
                reloadCompleteTime = currentTime + reloadSeconds;
            }

            return true;
        }

        public void Reset()
        {
            CurrentAmmo = magazineSize;
            reloadCompleteTime = float.PositiveInfinity;
        }
    }
}
