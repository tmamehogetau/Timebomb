using NUnit.Framework;
using Rounds2.Player;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class PlayerBotInputTests
    {
        [Test]
        public void DecideOffsetsAimAndWaitsForAllowedFireTime()
        {
            PlayerBotCommand command = PlayerBotInput.Decide(
                Vector2.zero,
                new Vector2(2f, 0f),
                timeSeconds: 0.5f,
                nextAllowedFireTimeSeconds: 1.25f);

            Assert.AreNotEqual(Vector2.right, command.Aim);
            Assert.IsFalse(command.ShouldFire);
        }

        [Test]
        public void DecideUsesNoticeableAimOffsetForTesting()
        {
            PlayerBotCommand command = PlayerBotInput.Decide(
                Vector2.zero,
                new Vector2(2f, 0f),
                timeSeconds: 0.5f,
                nextAllowedFireTimeSeconds: 0f);

            Assert.Greater(Vector2.Angle(Vector2.right, command.Aim), 12f);
        }

        [Test]
        public void DecideRequestsFireAfterAllowedFireTime()
        {
            PlayerBotCommand command = PlayerBotInput.Decide(
                Vector2.zero,
                new Vector2(2f, 0f),
                timeSeconds: 1.25f,
                nextAllowedFireTimeSeconds: 1.25f);

            Assert.IsTrue(command.ShouldFire);
        }

        [Test]
        public void DecideMovesTowardFarTarget()
        {
            PlayerBotCommand command = PlayerBotInput.Decide(
                Vector2.zero,
                new Vector2(5f, 0f),
                timeSeconds: 0f,
                nextAllowedFireTimeSeconds: 1.25f);

            Assert.Greater(command.Move.x, 0f);
        }

        [Test]
        public void DecideMovesAwayFromCloseTarget()
        {
            PlayerBotCommand command = PlayerBotInput.Decide(
                Vector2.zero,
                new Vector2(0.5f, 0f),
                timeSeconds: 0f,
                nextAllowedFireTimeSeconds: 1.25f);

            Assert.Less(command.Move.x, 0f);
        }
    }
}
