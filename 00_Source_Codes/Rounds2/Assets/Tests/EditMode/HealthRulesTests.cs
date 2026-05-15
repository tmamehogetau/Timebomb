using NUnit.Framework;
using Rounds2.Combat;
using Rounds2.Config;

namespace Rounds2.Tests.EditMode
{
    public sealed class HealthRulesTests
    {
        [Test]
        public void ResetHealthRestoresBaseHealth()
        {
            HealthState health = new();

            health.Reset(CombatTuning.BaseHealth);

            Assert.AreEqual(CombatTuning.BaseHealth, health.Current);
        }

        [Test]
        public void ApplyDamageClampsAtZeroAndRaisesDeathOnce()
        {
            HealthState health = new();
            int deathEvents = 0;

            health.Reset(CombatTuning.BaseHealth);
            if (health.ApplyDamage(CombatTuning.BaseHealth + 50))
            {
                deathEvents++;
            }

            if (health.ApplyDamage(CombatTuning.BulletDamage))
            {
                deathEvents++;
            }

            Assert.AreEqual(0, health.Current);
            Assert.IsTrue(health.IsDead);
            Assert.AreEqual(1, deathEvents);
        }
    }
}
