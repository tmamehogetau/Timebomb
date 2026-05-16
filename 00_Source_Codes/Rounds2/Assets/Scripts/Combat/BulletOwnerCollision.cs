using FishNet.Object;
using UnityEngine;

namespace Rounds2.Combat
{
    public static class BulletOwnerCollision
    {
        public static void Ignore(Collider2D[] bulletColliders, NetworkObject owner)
        {
            if (bulletColliders == null || owner == null)
            {
                return;
            }

            Collider2D[] ownerColliders = owner.GetComponentsInChildren<Collider2D>(true);
            foreach (Collider2D bulletCollider in bulletColliders)
            {
                if (bulletCollider == null)
                {
                    continue;
                }

                foreach (Collider2D ownerCollider in ownerColliders)
                {
                    if (ownerCollider != null)
                    {
                        Physics2D.IgnoreCollision(bulletCollider, ownerCollider, true);
                    }
                }
            }
        }
    }
}
