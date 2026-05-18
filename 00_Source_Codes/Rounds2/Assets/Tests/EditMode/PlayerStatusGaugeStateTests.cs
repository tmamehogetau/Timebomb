using NUnit.Framework;
using Rounds2.UI;

namespace Rounds2.Tests.EditMode
{
    public sealed class PlayerStatusGaugeStateTests
    {
        [Test]
        public void CreateConvertsAmmoCountToStackedAmmoSlots()
        {
            PlayerStatusGaugeState state = PlayerStatusGaugeState.Create(
                currentAmmo: 3,
                magazineSize: 6,
                weaponReloading: false,
                weaponReloadRemaining: 0f,
                weaponReloadSeconds: 1.5f,
                shieldCooldownRemaining: 0f,
                shieldCooldownSeconds: 4f);

            Assert.AreEqual(3, state.ActiveAmmoSlots);
            Assert.AreEqual(6, state.MagazineSize);
            Assert.AreEqual(0f, state.ReloadFill, 0.001f);
            Assert.AreEqual(1f, state.ShieldCooldownFill, 0.001f);
        }

        [Test]
        public void CreateShowsWeaponReloadGaugeFillingUp()
        {
            PlayerStatusGaugeState state = PlayerStatusGaugeState.Create(
                currentAmmo: 0,
                magazineSize: 6,
                weaponReloading: true,
                weaponReloadRemaining: 0.75f,
                weaponReloadSeconds: 1.5f,
                shieldCooldownRemaining: 2f,
                shieldCooldownSeconds: 4f);

            Assert.AreEqual(0, state.ActiveAmmoSlots);
            Assert.AreEqual(6, state.MagazineSize);
            Assert.AreEqual(0.5f, state.ReloadFill, 0.001f);
            Assert.AreEqual(0.5f, state.ShieldCooldownFill, 0.001f);
        }

        [Test]
        public void CreateClampsGaugeValues()
        {
            PlayerStatusGaugeState state = PlayerStatusGaugeState.Create(
                currentAmmo: 9,
                magazineSize: 6,
                weaponReloading: true,
                weaponReloadRemaining: -1f,
                weaponReloadSeconds: 1.5f,
                shieldCooldownRemaining: 9f,
                shieldCooldownSeconds: 4f);

            Assert.AreEqual(6, state.ActiveAmmoSlots);
            Assert.AreEqual(6, state.MagazineSize);
            Assert.AreEqual(1f, state.ReloadFill, 0.001f);
            Assert.AreEqual(0f, state.ShieldCooldownFill, 0.001f);
        }
    }
}
