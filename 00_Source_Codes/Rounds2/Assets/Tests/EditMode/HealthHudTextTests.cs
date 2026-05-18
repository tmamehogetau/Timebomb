using NUnit.Framework;
using Rounds2.UI;

namespace Rounds2.Tests.EditMode
{
    public sealed class HealthHudTextTests
    {
        [Test]
        public void FormatShowsBothPlayerHealthAndAmmo()
        {
            string text = HealthHudText.Format(
                100,
                75,
                100,
                6,
                3,
                6,
                leftReloading: false,
                rightReloading: false,
                "Ready",
                "Ready");

            Assert.AreEqual("P1 HP 100/100 Ammo 6/6 Shield Ready    P2 HP 75/100 Ammo 3/6 Shield Ready", text);
        }

        [Test]
        public void FormatClampsMissingPlayerHealthAtZero()
        {
            string text = HealthHudText.Format(
                100,
                -1,
                100,
                6,
                6,
                6,
                leftReloading: false,
                rightReloading: false,
                "Ready",
                "Ready");

            Assert.AreEqual("P1 HP 100/100 Ammo 6/6 Shield Ready    P2 HP 0/100 Ammo 6/6 Shield Ready", text);
        }

        [Test]
        public void FormatShowsReloadingState()
        {
            string text = HealthHudText.Format(
                100,
                75,
                100,
                6,
                0,
                6,
                leftReloading: false,
                rightReloading: true,
                "Ready",
                "2s");

            Assert.AreEqual("P1 HP 100/100 Ammo 6/6 Shield Ready    P2 HP 75/100 Ammo 0/6 Reloading Shield 2s", text);
        }

        [Test]
        public void FormatCanShowDifferentMagazineSizesAfterCards()
        {
            string text = HealthHudText.Format(
                100,
                75,
                100,
                6,
                7,
                6,
                8,
                leftReloading: false,
                rightReloading: false,
                "Ready",
                "Ready");

            Assert.AreEqual("P1 HP 100/100 Ammo 6/6 Shield Ready    P2 HP 75/100 Ammo 7/8 Shield Ready", text);
        }
    }
}
