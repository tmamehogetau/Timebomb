using NUnit.Framework;
using Rounds2.Combat;
using Rounds2.Config;

namespace Rounds2.Tests.EditMode
{
    public sealed class WeaponAmmoStateTests
    {
        [Test]
        public void StartsWithFullMagazine()
        {
            WeaponAmmoState ammo = new(CombatTuning.MagazineSize, CombatTuning.ReloadSeconds);

            Assert.AreEqual(CombatTuning.MagazineSize, ammo.CurrentAmmo);
            Assert.IsFalse(ammo.IsReloading);
        }

        [Test]
        public void ConsumesShotsAndStartsAutoReloadWhenMagazineEmpties()
        {
            WeaponAmmoState ammo = new(CombatTuning.MagazineSize, CombatTuning.ReloadSeconds);

            for (int i = 0; i < CombatTuning.MagazineSize; i++)
            {
                Assert.IsTrue(ammo.TryConsumeShot(i * CombatTuning.FireIntervalSeconds));
            }

            Assert.AreEqual(0, ammo.CurrentAmmo);
            Assert.IsTrue(ammo.IsReloading);
            Assert.AreEqual(CombatTuning.ReloadSeconds, ammo.ReloadRemaining((CombatTuning.MagazineSize - 1) * CombatTuning.FireIntervalSeconds), 0.001f);
        }

        [Test]
        public void BlocksShotsDuringReloadAndAllowsShotAfterReloadCompletes()
        {
            WeaponAmmoState ammo = new(CombatTuning.MagazineSize, CombatTuning.ReloadSeconds);
            float lastShotTime = 0f;

            for (int i = 0; i < CombatTuning.MagazineSize; i++)
            {
                lastShotTime = i * CombatTuning.FireIntervalSeconds;
                Assert.IsTrue(ammo.TryConsumeShot(lastShotTime));
            }

            Assert.IsFalse(ammo.TryConsumeShot(lastShotTime + CombatTuning.ReloadSeconds - 0.01f));
            Assert.IsTrue(ammo.TryConsumeShot(lastShotTime + CombatTuning.ReloadSeconds));
            Assert.AreEqual(CombatTuning.MagazineSize - 1, ammo.CurrentAmmo);
            Assert.IsFalse(ammo.IsReloading);
        }

        [Test]
        public void ResetRefillsMagazineAndClearsReload()
        {
            WeaponAmmoState ammo = new(CombatTuning.MagazineSize, CombatTuning.ReloadSeconds);

            for (int i = 0; i < CombatTuning.MagazineSize; i++)
            {
                Assert.IsTrue(ammo.TryConsumeShot(i * CombatTuning.FireIntervalSeconds));
            }

            ammo.Reset();

            Assert.AreEqual(CombatTuning.MagazineSize, ammo.CurrentAmmo);
            Assert.IsFalse(ammo.IsReloading);
            Assert.AreEqual(0f, ammo.ReloadRemaining(100f), 0.001f);
        }

        [Test]
        public void ResetCanApplyCardModifiedMagazineAndReloadSeconds()
        {
            WeaponAmmoState ammo = new(CombatTuning.MagazineSize, CombatTuning.ReloadSeconds);

            ammo.Reset(magazineSize: 8, reloadSeconds: 0.75f);
            for (int i = 0; i < 8; i++)
            {
                Assert.IsTrue(ammo.TryConsumeShot(i));
            }

            Assert.AreEqual(8, ammo.MagazineSize);
            Assert.AreEqual(0, ammo.CurrentAmmo);
            Assert.AreEqual(0.75f, ammo.ReloadRemaining(7f), 0.001f);
        }

        [Test]
        public void CanConsumeMultipleAmmoForBurstCosts()
        {
            WeaponAmmoState ammo = new(magazineSize: 5, reloadSeconds: 1.25f);

            Assert.IsTrue(ammo.TryConsumeShots(currentTime: 0f, shotCost: 3, out int firstConsumedShots));

            Assert.AreEqual(3, firstConsumedShots);
            Assert.AreEqual(2, ammo.CurrentAmmo);
            Assert.IsFalse(ammo.IsReloading);

            Assert.IsTrue(ammo.TryConsumeShots(currentTime: 0.2f, shotCost: 3, out int secondConsumedShots));
            Assert.AreEqual(2, secondConsumedShots);
            Assert.AreEqual(0, ammo.CurrentAmmo);
            Assert.IsTrue(ammo.IsReloading);
        }

        [Test]
        public void ConsumesExactlyTheFullMultiShotCost()
        {
            WeaponAmmoState ammo = new(magazineSize: 9, reloadSeconds: 1.25f);

            Assert.IsTrue(ammo.TryConsumeShots(currentTime: 0f, shotCost: 9));

            Assert.AreEqual(0, ammo.CurrentAmmo);
            Assert.IsTrue(ammo.IsReloading);
        }
    }
}
