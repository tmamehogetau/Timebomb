using NUnit.Framework;
using Rounds2.Player;

namespace Rounds2.Tests.EditMode
{
    public sealed class PlayerShieldStateTests
    {
        [Test]
        public void TryActivateStartsShieldAndCooldown()
        {
            PlayerShieldState shield = new(activeSeconds: 0.35f, cooldownSeconds: 4f);

            Assert.IsTrue(shield.TryActivate(10f));

            Assert.IsTrue(shield.IsShielding(10.2f));
            Assert.IsFalse(shield.IsReady(10.2f));
            Assert.AreEqual(3.8f, shield.CooldownRemaining(10.2f), 0.001f);
        }

        [Test]
        public void ShieldExpiresBeforeCooldownCompletes()
        {
            PlayerShieldState shield = new(activeSeconds: 0.35f, cooldownSeconds: 4f);
            shield.TryActivate(10f);

            Assert.IsFalse(shield.IsShielding(10.36f));
            Assert.IsFalse(shield.IsReady(10.36f));
            Assert.AreEqual(3.64f, shield.CooldownRemaining(10.36f), 0.001f);
        }

        [Test]
        public void TryActivateRejectsCooldownAndAllowsAfterCooldown()
        {
            PlayerShieldState shield = new(activeSeconds: 0.35f, cooldownSeconds: 4f);
            shield.TryActivate(10f);

            Assert.IsFalse(shield.TryActivate(13.9f));
            Assert.IsTrue(shield.TryActivate(14f));
        }

        [Test]
        public void ResetMakesShieldReady()
        {
            PlayerShieldState shield = new(activeSeconds: 0.35f, cooldownSeconds: 4f);
            shield.TryActivate(10f);

            shield.Reset();

            Assert.IsFalse(shield.IsShielding(10.1f));
            Assert.IsTrue(shield.IsReady(10.1f));
            Assert.AreEqual(0f, shield.CooldownRemaining(10.1f), 0.001f);
        }

        [Test]
        public void ResetCanApplyCardModifiedShieldTiming()
        {
            PlayerShieldState shield = new(activeSeconds: 0.35f, cooldownSeconds: 4f);

            shield.Reset(activeSeconds: 0.5f, cooldownSeconds: 3f);
            shield.TryActivate(10f);

            Assert.IsTrue(shield.IsShielding(10.49f));
            Assert.IsFalse(shield.IsShielding(10.51f));
            Assert.IsFalse(shield.IsReady(12.99f));
            Assert.IsTrue(shield.IsReady(13f));
        }
    }
}
