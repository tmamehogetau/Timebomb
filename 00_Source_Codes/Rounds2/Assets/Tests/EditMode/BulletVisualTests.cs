using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using System;
using System.IO;

namespace Rounds2.Tests.EditMode
{
    public sealed class BulletVisualTests
    {
        [Test]
        public void BulletSpriteUsesSmallWorldScale()
        {
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Prefabs/BulletSprite.png");
            GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Bullet.prefab");

            Assert.NotNull(importer);
            Assert.NotNull(bulletPrefab);
            Assert.AreEqual(64f, importer.spritePixelsPerUnit, 0.001f);
            Assert.AreEqual(0.12f, bulletPrefab.GetComponent<CircleCollider2D>().radius, 0.001f);
        }

        [Test]
        public void BulletCreatesShortSubtleTrailAtRuntime()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Combat", "Bullet.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("TrailRenderer", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("ConfigureTrailRenderer", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("CombatTuning.BulletTrailSeconds", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("CombatTuning.BulletTrailStartWidth", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("CombatTuning.BulletTrailEndWidth", StringComparison.Ordinal));
        }

        [Test]
        public void BulletTrailWaitsForNetworkSpawnPositionBeforeEmitting()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Combat", "Bullet.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("EnableTrailAfterSpawnFrame", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("trailRenderer.emitting = false", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("yield return null", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("trailRenderer.emitting = true", StringComparison.Ordinal));
        }
    }
}
