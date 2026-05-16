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
        }
    }
}
