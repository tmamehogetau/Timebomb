using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

namespace Rounds2.Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlayerOwnerVisuals : NetworkBehaviour
    {
        [SerializeField] private Color ownerColor = new(0.35f, 1f, 0.45f, 1f);
        [SerializeField] private Color remoteColor = new(0.2f, 0.8f, 1f, 1f);

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            ApplyColor();
        }

        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            base.OnOwnershipClient(prevOwner);
            ApplyColor();
        }

        private void ApplyColor()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.color = IsOwner ? ownerColor : remoteColor;
        }
    }
}
