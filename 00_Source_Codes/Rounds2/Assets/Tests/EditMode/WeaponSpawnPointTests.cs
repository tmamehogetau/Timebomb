using NUnit.Framework;
using Rounds2.Combat;
using Rounds2.Config;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class WeaponSpawnPointTests
    {
        [Test]
        public void FromShooterPlacesBulletOnAimLine()
        {
            Vector2 spawn = WeaponSpawnPoint.FromShooter(new Vector2(2f, 3f), Vector2.up);
            float expectedForwardOffset = CombatTuning.MuzzleForwardOffset + CombatTuning.AimIndicatorLength * 0.5f;

            Assert.AreEqual(new Vector2(2f, 3f + expectedForwardOffset), spawn);
            Assert.Greater(expectedForwardOffset, CombatTuning.MuzzleForwardOffset);
            Assert.Less(expectedForwardOffset, CombatTuning.MuzzleForwardOffset + CombatTuning.AimIndicatorLength);
        }

        [Test]
        public void FromShooterNormalizesAimDirection()
        {
            Vector2 spawn = WeaponSpawnPoint.FromShooter(Vector2.zero, new Vector2(0f, 10f));
            float expectedForwardOffset = CombatTuning.MuzzleForwardOffset + CombatTuning.AimIndicatorLength * 0.5f;

            Assert.AreEqual(new Vector2(0f, expectedForwardOffset), spawn);
        }

        [Test]
        public void ResolveRequestedSpawnAcceptsNearbyOwnerPrediction()
        {
            Vector2 serverSpawn = Vector2.zero;
            Vector2 requestedSpawn = new(0f, 0.25f);

            Assert.AreEqual(requestedSpawn, WeaponSpawnPoint.ResolveRequestedSpawn(serverSpawn, requestedSpawn, Vector2.up));
        }

        [Test]
        public void ResolveRequestedSpawnRejectsDistantOwnerPrediction()
        {
            Vector2 serverSpawn = Vector2.zero;
            Vector2 requestedSpawn = new(10f, 0f);

            Assert.AreEqual(serverSpawn, WeaponSpawnPoint.ResolveRequestedSpawn(serverSpawn, requestedSpawn, Vector2.right));
        }

        [Test]
        public void ResolveRequestedSpawnKeepsNearbyOwnerPredictionDuringSideMovement()
        {
            Vector2 serverSpawn = new(2f, 3f);
            Vector2 requestedSpawn = serverSpawn + new Vector2(0.75f, 0.25f);

            Vector2 resolved = WeaponSpawnPoint.ResolveRequestedSpawn(serverSpawn, requestedSpawn, Vector2.up);

            Assert.AreEqual(requestedSpawn.x, resolved.x, 0.001f);
            Assert.AreEqual(requestedSpawn.y, resolved.y, 0.001f);
        }
    }
}
