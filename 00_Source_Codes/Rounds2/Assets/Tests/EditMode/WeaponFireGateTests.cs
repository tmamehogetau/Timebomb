using NUnit.Framework;
using Rounds2.Combat;
using Rounds2.Config;

namespace Rounds2.Tests.EditMode
{
    public sealed class WeaponFireGateTests
    {
        [Test]
        public void TryConsumeShotAllowsFirstShot()
        {
            WeaponFireGate gate = new();

            Assert.IsTrue(gate.TryConsumeShot(10f));
        }

        [Test]
        public void TryConsumeShotBlocksUntilFireIntervalPasses()
        {
            WeaponFireGate gate = new();

            Assert.IsTrue(gate.TryConsumeShot(10f));
            Assert.IsFalse(gate.TryConsumeShot(10f + CombatTuning.FireIntervalSeconds - 0.01f));
            Assert.IsTrue(gate.TryConsumeShot(10f + CombatTuning.FireIntervalSeconds));
        }

        [Test]
        public void CanConsumeShotDoesNotAdvanceFireGate()
        {
            WeaponFireGate gate = new();

            Assert.IsTrue(gate.CanConsumeShot(10f));
            Assert.IsTrue(gate.CanConsumeShot(10f));
            gate.ConsumeShot(10f);

            Assert.IsFalse(gate.CanConsumeShot(10f + CombatTuning.FireIntervalSeconds - 0.01f));
            Assert.IsTrue(gate.CanConsumeShot(10f + CombatTuning.FireIntervalSeconds));
        }

        [Test]
        public void ResetClearsCooldownSoNextRoundCanFireImmediately()
        {
            WeaponFireGate gate = new();

            gate.ConsumeShot(10f);
            gate.Reset();

            Assert.IsTrue(gate.CanConsumeShot(10f + 0.01f));
        }
    }
}
