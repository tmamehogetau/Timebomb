using NUnit.Framework;
using Rounds2.UI;

namespace Rounds2.Tests.EditMode
{
    public sealed class HealthHudTextTests
    {
        [Test]
        public void FormatShowsBothPlayerHealth()
        {
            string text = HealthHudText.Format(100, 75, 100);

            Assert.AreEqual("P1 HP 100/100    P2 HP 75/100", text);
        }

        [Test]
        public void FormatClampsMissingPlayerHealthAtZero()
        {
            string text = HealthHudText.Format(100, -1, 100);

            Assert.AreEqual("P1 HP 100/100    P2 HP 0/100", text);
        }
    }
}
