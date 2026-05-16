using FishNet.Object;
using NUnit.Framework;
using Rounds2.Combat;
using Rounds2.Config;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class BulletOwnerCollisionTests
    {
        [Test]
        public void LaunchIgnoresOwnerCollider()
        {
            GameObject ownerObject = new("Owner");
            NetworkObject owner = ownerObject.AddComponent<NetworkObject>();
            Collider2D ownerCollider = ownerObject.AddComponent<CircleCollider2D>();

            GameObject bulletObject = new("Bullet");
            Rigidbody2D body = bulletObject.AddComponent<Rigidbody2D>();
            Collider2D bulletCollider = bulletObject.AddComponent<CircleCollider2D>();
            Bullet bullet = bulletObject.AddComponent<Bullet>();

            bullet.Launch(owner, Vector2.right);

            Assert.IsTrue(Physics2D.GetIgnoreCollision(bulletCollider, ownerCollider));
            Assert.AreEqual(CombatTuning.BulletSpeed, body.linearVelocity.x, 0.001f);
            Assert.AreEqual(0f, body.linearVelocity.y, 0.001f);

            Object.DestroyImmediate(bulletObject);
            Object.DestroyImmediate(ownerObject);
        }

        [Test]
        public void LaunchIgnoresOwnerChildColliders()
        {
            GameObject ownerObject = new("Owner");
            NetworkObject owner = ownerObject.AddComponent<NetworkObject>();

            GameObject childColliderObject = new("OwnerCollider");
            childColliderObject.transform.SetParent(ownerObject.transform, false);
            Collider2D ownerCollider = childColliderObject.AddComponent<CircleCollider2D>();

            GameObject bulletObject = new("Bullet");
            bulletObject.AddComponent<Rigidbody2D>();
            Collider2D bulletCollider = bulletObject.AddComponent<CircleCollider2D>();
            Bullet bullet = bulletObject.AddComponent<Bullet>();

            bullet.Launch(owner, Vector2.right);

            Assert.IsTrue(Physics2D.GetIgnoreCollision(bulletCollider, ownerCollider));

            Object.DestroyImmediate(bulletObject);
            Object.DestroyImmediate(ownerObject);
        }
    }
}
