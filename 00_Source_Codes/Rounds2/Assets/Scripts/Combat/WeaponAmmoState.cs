using System;

namespace Rounds2.Combat
{
    public sealed class WeaponAmmoState
    {
        private float reloadCompleteTime = float.PositiveInfinity;

        public WeaponAmmoState(int magazineSize, float reloadSeconds)
        {
            Configure(magazineSize, reloadSeconds);
            CurrentAmmo = MagazineSize;
        }

        public int MagazineSize { get; private set; }
        public float ReloadSeconds { get; private set; }
        public int CurrentAmmo { get; private set; }
        public bool IsReloading => !float.IsPositiveInfinity(reloadCompleteTime);

        private void Configure(int magazineSize, float reloadSeconds)
        {
            if (magazineSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(magazineSize));
            }

            if (reloadSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(reloadSeconds));
            }

            MagazineSize = magazineSize;
            ReloadSeconds = reloadSeconds;
        }

        public float ReloadRemaining(float currentTime)
        {
            return IsReloading ? Math.Max(0f, reloadCompleteTime - currentTime) : 0f;
        }

        public bool UpdateReload(float currentTime)
        {
            if (!IsReloading || currentTime < reloadCompleteTime)
            {
                return false;
            }

            CurrentAmmo = MagazineSize;
            reloadCompleteTime = float.PositiveInfinity;
            return true;
        }

        public bool TryConsumeShot(float currentTime)
        {
            return TryConsumeShots(currentTime, shotCost: 1);
        }

        public bool TryConsumeShots(float currentTime, int shotCost)
        {
            return TryConsumeShots(currentTime, shotCost, out _);
        }

        public bool TryConsumeShots(float currentTime, int shotCost, out int consumedShots)
        {
            UpdateReload(currentTime);
            consumedShots = 0;

            if (shotCost <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(shotCost));
            }

            if (IsReloading || CurrentAmmo <= 0)
            {
                return false;
            }

            consumedShots = Math.Min(CurrentAmmo, shotCost);
            CurrentAmmo -= consumedShots;
            if (CurrentAmmo == 0)
            {
                StartReload(currentTime);
            }

            return consumedShots > 0;
        }

        private void StartReload(float currentTime)
        {
            reloadCompleteTime = currentTime + ReloadSeconds;
        }

        public void Reset()
        {
            CurrentAmmo = MagazineSize;
            reloadCompleteTime = float.PositiveInfinity;
        }

        public void Reset(int magazineSize, float reloadSeconds)
        {
            Configure(magazineSize, reloadSeconds);
            CurrentAmmo = MagazineSize;
            reloadCompleteTime = float.PositiveInfinity;
        }
    }
}
