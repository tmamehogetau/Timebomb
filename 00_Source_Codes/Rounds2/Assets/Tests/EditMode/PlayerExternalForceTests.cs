using NUnit.Framework;
using Rounds2.Player;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class PlayerExternalForceTests
    {
        [Test]
        public void CreateNormalizesDirectionAndBuildsVelocity()
        {
            PlayerExternalForce force = PlayerExternalForce.Create(new Vector2(3f, 4f), 10f, 2);

            Assert.AreEqual(6f, force.Velocity.x, 0.001f);
            Assert.AreEqual(8f, force.Velocity.y, 0.001f);
            Assert.AreEqual(2u, force.RemainingTicks);
            Assert.IsTrue(force.IsActive);
        }

        [Test]
        public void QueueStacksForcesAndExpiresThemByTick()
        {
            PlayerExternalForceQueue queue = new();
            queue.Enqueue(PlayerExternalForce.Create(Vector2.right, 3f, 2));
            queue.Enqueue(PlayerExternalForce.Create(Vector2.up, 4f, 1));

            Vector2 firstTick = queue.ConsumeTickVelocity();
            Vector2 secondTick = queue.ConsumeTickVelocity();
            Vector2 thirdTick = queue.ConsumeTickVelocity();

            Assert.AreEqual(new Vector2(3f, 4f), firstTick);
            Assert.AreEqual(new Vector2(1.5f, 0f), secondTick);
            Assert.AreEqual(Vector2.zero, thirdTick);
        }

        [Test]
        public void ForceVelocityDecaysAcrossRemainingTicks()
        {
            PlayerExternalForce force = PlayerExternalForce.Create(Vector2.right, 8f, 4);

            Assert.AreEqual(8f, force.Velocity.x, 0.001f);
            force = force.ConsumeTick();
            Assert.AreEqual(6f, force.Velocity.x, 0.001f);
            force = force.ConsumeTick();
            Assert.AreEqual(4f, force.Velocity.x, 0.001f);
            force = force.ConsumeTick();
            Assert.AreEqual(2f, force.Velocity.x, 0.001f);
            force = force.ConsumeTick();
            Assert.IsFalse(force.IsActive);
        }

        [Test]
        public void QueueIgnoresInactiveForces()
        {
            PlayerExternalForceQueue queue = new();
            queue.Enqueue(PlayerExternalForce.Create(Vector2.zero, 10f, 2));
            queue.Enqueue(PlayerExternalForce.Create(Vector2.right, 0f, 2));
            queue.Enqueue(PlayerExternalForce.Create(Vector2.right, 10f, 0));

            Assert.AreEqual(Vector2.zero, queue.ConsumeTickVelocity());
        }
    }
}
