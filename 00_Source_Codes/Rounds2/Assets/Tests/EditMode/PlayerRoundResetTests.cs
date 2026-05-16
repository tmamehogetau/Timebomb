using NUnit.Framework;
using Rounds2.Player;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class PlayerRoundResetTests
    {
        [Test]
        public void ApplyMovesRigidbodyAndClearsVelocity()
        {
            GameObject player = new("Player");
            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            body.linearVelocity = new Vector2(3f, -2f);
            body.angularVelocity = 45f;

            PlayerRoundReset.Apply(player.transform, body, new Vector3(2f, -1f, 0f), Quaternion.Euler(0f, 0f, 90f));

            Assert.AreEqual(new Vector2(2f, -1f), body.position);
            Assert.AreEqual(Vector2.zero, body.linearVelocity);
            Assert.AreEqual(0f, body.angularVelocity);
            Assert.AreEqual(90f, body.rotation, 0.001f);

            Object.DestroyImmediate(player);
        }

        [Test]
        public void ApplyMovesTransformWithoutRigidbody()
        {
            GameObject player = new("Player");

            PlayerRoundReset.Apply(player.transform, null, new Vector3(-4f, 1f, 0f), Quaternion.Euler(0f, 0f, 180f));

            Assert.AreEqual(new Vector3(-4f, 1f, 0f), player.transform.position);
            Assert.AreEqual(180f, player.transform.eulerAngles.z, 0.001f);

            Object.DestroyImmediate(player);
        }
    }
}
