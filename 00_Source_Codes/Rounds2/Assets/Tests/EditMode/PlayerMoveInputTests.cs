using NUnit.Framework;
using Rounds2.Player;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class PlayerMoveInputTests
    {
        [Test]
        public void CreateClampsMoveAndNormalizesAim()
        {
            PlayerMoveInput input = PlayerMoveInput.Create(new Vector2(2f, 2f), new Vector2(0f, 4f));

            Assert.LessOrEqual(input.Move.magnitude, 1.001f);
            Assert.AreEqual(Vector2.up.x, input.Aim.x, 0.001f);
            Assert.AreEqual(Vector2.up.y, input.Aim.y, 0.001f);
        }

        [Test]
        public void CreateKeepsFallbackAimWhenRequestedAimIsZero()
        {
            PlayerMoveInput input = PlayerMoveInput.Create(Vector2.zero, Vector2.zero, Vector2.left);

            Assert.AreEqual(Vector2.left.x, input.Aim.x, 0.001f);
            Assert.AreEqual(Vector2.left.y, input.Aim.y, 0.001f);
        }
    }
}
