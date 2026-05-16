using System.Collections.Generic;
using UnityEngine;

namespace Rounds2.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerContactCollision : MonoBehaviour
    {
        private static readonly List<PlayerContactCollision> ActivePlayers = new();

        private Collider2D[] colliders;

        private void Awake()
        {
            CacheColliders();
        }

        private void OnEnable()
        {
            RegisterWithOtherPlayers();
        }

        private void OnDisable()
        {
            ActivePlayers.Remove(this);
        }

        public void RegisterWithOtherPlayers()
        {
            CacheColliders();
            foreach (PlayerContactCollision other in ActivePlayers)
            {
                IgnoreContactWith(other);
            }

            if (!ActivePlayers.Contains(this))
            {
                ActivePlayers.Add(this);
            }
        }

        private void IgnoreContactWith(PlayerContactCollision other)
        {
            other.CacheColliders();
            foreach (Collider2D ownCollider in colliders)
            {
                foreach (Collider2D otherCollider in other.colliders)
                {
                    if (ownCollider != null && otherCollider != null)
                    {
                        Physics2D.IgnoreCollision(ownCollider, otherCollider, true);
                    }
                }
            }
        }

        private void CacheColliders()
        {
            colliders ??= GetComponentsInChildren<Collider2D>();
        }
    }
}
