using NUnit.Framework;
using Rounds2.Player;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class PlayerSeparationRulesTests
    {
        [Test]
        public void FilterVelocityLeavesVelocityWhenPlayersRemainSeparated()
        {
            Vector2 velocity = PlayerSeparationRules.FilterVelocity(
                selfPosition: Vector2.zero,
                otherPosition: new Vector2(2f, 0f),
                desiredVelocity: Vector2.right,
                minimumDistance: 0.95f,
                deltaTime: 0.02f);

            Assert.AreEqual(Vector2.right, velocity);
        }

        [Test]
        public void FilterVelocityRemovesInwardVelocityBeforeOverlap()
        {
            Vector2 velocity = PlayerSeparationRules.FilterVelocity(
                selfPosition: Vector2.zero,
                otherPosition: new Vector2(1f, 0f),
                desiredVelocity: new Vector2(4f, 1f),
                minimumDistance: 0.95f,
                deltaTime: 0.02f);

            Assert.AreEqual(new Vector2(0f, 1f), velocity);
        }

        [Test]
        public void FilterVelocityKeepsOutwardVelocityWhenAlreadyClose()
        {
            Vector2 velocity = PlayerSeparationRules.FilterVelocity(
                selfPosition: Vector2.zero,
                otherPosition: new Vector2(0.5f, 0f),
                desiredVelocity: new Vector2(-2f, 1f),
                minimumDistance: 0.95f,
                deltaTime: 0.02f);

            Assert.AreEqual(new Vector2(-2f, 1f), velocity);
        }

        [Test]
        public void FilterVelocityRemovesInwardVelocityWhenAlreadyClose()
        {
            Vector2 velocity = PlayerSeparationRules.FilterVelocity(
                selfPosition: Vector2.zero,
                otherPosition: new Vector2(0.5f, 0f),
                desiredVelocity: new Vector2(2f, 1f),
                minimumDistance: 0.95f,
                deltaTime: 0.02f);

            Assert.AreEqual(new Vector2(0f, 1f), velocity);
        }
    }
}
