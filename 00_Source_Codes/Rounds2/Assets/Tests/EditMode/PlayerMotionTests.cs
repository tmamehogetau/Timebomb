using NUnit.Framework;
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
    }
}
