using NUnit.Framework;
using Rounds2.Combat;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class HealthVisualsTests
    {
        [Test]
        public void SetAliveDisablesRendererAndCollidersWhenDead()
        {
            GameObject player = new("Player");
            SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
            SpriteRenderer childRenderer = CreateChildRenderer(player);
            Collider2D collider = player.AddComponent<CircleCollider2D>();
            HealthVisuals visuals = player.AddComponent<HealthVisuals>();

            visuals.SetAlive(false);

            Assert.IsFalse(renderer.enabled);
            Assert.IsFalse(childRenderer.enabled);
            Assert.IsFalse(collider.enabled);

            Object.DestroyImmediate(player);
        }

        [Test]
        public void SetAliveRestoresRendererAndCollidersWhenAlive()
        {
            GameObject player = new("Player");
            SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
            SpriteRenderer childRenderer = CreateChildRenderer(player);
            Collider2D collider = player.AddComponent<CircleCollider2D>();
            HealthVisuals visuals = player.AddComponent<HealthVisuals>();

            visuals.SetAlive(false);
            visuals.SetAlive(true);

            Assert.IsTrue(renderer.enabled);
            Assert.IsTrue(childRenderer.enabled);
            Assert.IsTrue(collider.enabled);

            Object.DestroyImmediate(player);
        }

        private static SpriteRenderer CreateChildRenderer(GameObject parent)
        {
            GameObject child = new("AimIndicator");
            child.transform.SetParent(parent.transform, false);
            return child.AddComponent<SpriteRenderer>();
        }
    }
}
