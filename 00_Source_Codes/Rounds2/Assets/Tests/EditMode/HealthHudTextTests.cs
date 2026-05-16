using NUnit.Framework;
using Rounds2.UI;

namespace Rounds2.Tests.EditMode
{
    public sealed class HealthHudTextTests
    {
        [Test]
        public void FormatShowsBothPlayerHealthAndAmmo()
        {
            string text = HealthHudText.Format(100, 75, 100, 6, 3, 6, leftReloading: false, rightReloading: false);

            Assert.AreEqual("P1 HP 100/100 Ammo 6/6    P2 HP 75/100 Ammo 3/6", text);
        }

        [Test]
        public void FormatClampsMissingPlayerHealthAtZero()
        {
            string text = HealthHudText.Format(100, -1, 100, 6, 6, 6, leftReloading: false, rightReloading: false);

            Assert.AreEqual("P1 HP 100/100 Ammo 6/6    P2 HP 0/100 Ammo 6/6", text);
        }

        [Test]
        public void FormatShowsReloadingState()
        {
            string text = HealthHudText.Format(100, 75, 100, 6, 0, 6, leftReloading: false, rightReloading: true);

            Assert.AreEqual("P1 HP 100/100 Ammo 6/6    P2 HP 75/100 Ammo 0/6 Reloading", text);
        }
    }
}
