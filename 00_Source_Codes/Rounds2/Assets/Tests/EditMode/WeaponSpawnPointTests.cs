using NUnit.Framework;
using Rounds2.Combat;
using Rounds2.Config;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class WeaponSpawnPointTests
    {
        [Test]
        public void FromShooterPlacesMuzzleOnAimDirection()
        {
            Vector2 spawn = WeaponSpawnPoint.FromShooter(new Vector2(2f, 3f), Vector2.up);

            Assert.AreEqual(new Vector2(2f, 3f + CombatTuning.MuzzleForwardOffset), spawn);
        }

        [Test]
        public void FromShooterNormalizesAimDirection()
        {
            Vector2 spawn = WeaponSpawnPoint.FromShooter(Vector2.zero, new Vector2(0f, 10f));

            Assert.AreEqual(new Vector2(0f, CombatTuning.MuzzleForwardOffset), spawn);
        }

        [Test]
        public void ResolveRequestedSpawnAcceptsNearbyOwnerPrediction()
        {
            Vector2 serverSpawn = Vector2.zero;
            Vector2 requestedSpawn = new(0.25f, 0.1f);

            Assert.AreEqual(requestedSpawn, WeaponSpawnPoint.ResolveRequestedSpawn(serverSpawn, requestedSpawn));
        }

        [Test]
        public void ResolveRequestedSpawnRejectsDistantOwnerPrediction()
        {
            Vector2 serverSpawn = Vector2.zero;
            Vector2 requestedSpawn = new(10f, 0f);

            Assert.AreEqual(serverSpawn, WeaponSpawnPoint.ResolveRequestedSpawn(serverSpawn, requestedSpawn));
        }
    }
}
