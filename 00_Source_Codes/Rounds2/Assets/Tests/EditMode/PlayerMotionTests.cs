using NUnit.Framework;
using Rounds2.Config;
using Rounds2.Player;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class PlayerMotionTests
    {
        [Test]
        public void ClampMoveInputLimitsDiagonalMagnitude()
        {
            Vector2 clamped = PlayerMotion.ClampMoveInput(new Vector2(1f, 1f));

            Assert.LessOrEqual(clamped.magnitude, 1.0001f);
            Assert.AreEqual(clamped.x, clamped.y, 0.0001f);
        }

        [Test]
        public void AimAngleDegreesFacesAimDirection()
        {
            Assert.AreEqual(0f, PlayerMotion.AimAngleDegrees(Vector2.right), 0.0001f);
            Assert.AreEqual(90f, PlayerMotion.AimAngleDegrees(Vector2.up), 0.0001f);
            Assert.AreEqual(180f, Mathf.Abs(PlayerMotion.AimAngleDegrees(Vector2.left)), 0.0001f);
        }

        [Test]
        public void SmoothMoveVelocityAcceleratesWithoutSnapping()
        {
            Vector2 next = PlayerMotion.SmoothMoveVelocity(
                Vector2.zero,
                Vector2.right * CombatTuning.MoveSpeed,
                1f / 60f,
                CombatTuning.MoveAcceleration,
                CombatTuning.MoveDeceleration);

            Assert.Greater(next.x, 0f);
            Assert.Less(next.x, CombatTuning.MoveSpeed);
            Assert.AreEqual(0f, next.y, 0.0001f);
        }

        [Test]
        public void SmoothMoveVelocityDeceleratesTowardStop()
        {
            Vector2 next = PlayerMotion.SmoothMoveVelocity(
                Vector2.right * CombatTuning.MoveSpeed,
                Vector2.zero,
                1f / 60f,
                CombatTuning.MoveAcceleration,
                CombatTuning.MoveDeceleration);

            Assert.Greater(next.x, 0f);
            Assert.Less(next.x, CombatTuning.MoveSpeed);
            Assert.AreEqual(0f, next.y, 0.0001f);
        }
    }
}
