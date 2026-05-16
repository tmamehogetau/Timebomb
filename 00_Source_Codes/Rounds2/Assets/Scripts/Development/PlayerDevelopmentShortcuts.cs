using FishNet.Object;
using Rounds2.Match;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rounds2.Development
{
    [DisallowMultipleComponent]
    public sealed class PlayerDevelopmentShortcuts : NetworkBehaviour
    {
        private static readonly Vector2 StatusSize = new(360f, 28f);

        private void Update()
        {
            if (!IsOwner)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.f2Key.wasPressedThisFrame)
            {
                bool enabled = DevelopmentRuntimeOptions.ToggleBot();
                Debug.Log($"Rounds2 bot {(enabled ? "enabled" : "disabled")} by F2.");
            }

            if (keyboard.f5Key.wasPressedThisFrame)
            {
                RequestMatchResetServerRpc();
            }
        }

        private void OnGUI()
        {
            if (!IsOwner)
            {
                return;
            }

            Rect statusRect = new(16f, Screen.height - 44f, StatusSize.x, StatusSize.y);
            GUI.Label(statusRect, DevelopmentRuntimeOptions.StatusText);
        }

        [ServerRpc]
        private void RequestMatchResetServerRpc()
        {
            SetManager setManager = FindFirstObjectByType<SetManager>();
            if (setManager != null)
            {
                setManager.ResetMatch();
            }
        }
    }
}
