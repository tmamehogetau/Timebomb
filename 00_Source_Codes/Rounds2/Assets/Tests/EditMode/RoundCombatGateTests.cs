using NUnit.Framework;
using Rounds2.Match;

namespace Rounds2.Tests.EditMode
{
    public sealed class RoundCombatGateTests
    {
        [TearDown]
        public void TearDown()
        {
            RoundCombatGate.Open();
        }

        [Test]
        public void CloseBlocksCombatUntilOpened()
        {
            RoundCombatGate.Open();

            RoundCombatGate.Close();

            Assert.IsFalse(RoundCombatGate.IsOpen);

            RoundCombatGate.Open();

            Assert.IsTrue(RoundCombatGate.IsOpen);
        }
    }
}
