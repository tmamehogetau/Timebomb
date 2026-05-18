using FishNet.Object;
using FishNet.Connection;
using Rounds2.Cards;
using Rounds2.Combat;
using Rounds2.Match;
using Rounds2.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rounds2.Development
{
    [DisallowMultipleComponent]
    public sealed class PlayerDevelopmentShortcuts : NetworkBehaviour
    {
        private static readonly Vector2 StatusSize = new(1200f, 20f);
        private string cardStatsText = DevelopmentCardStatsText.Format(CombatCardStats.Base, 0);

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

            if (keyboard.f6Key.wasPressedThisFrame)
            {
                RequestGrantCardServerRpc(DevelopmentRuntimeOptions.SelectedCard);
            }

            if (keyboard.f7Key.wasPressedThisFrame)
            {
                CardId selectedCard = DevelopmentRuntimeOptions.SelectNextCard();
                Debug.Log($"Rounds2 dev selected card {CardText.NameWithEffect(selectedCard)}.");
            }

            if (keyboard.f8Key.wasPressedThisFrame)
            {
                RequestClearCardsServerRpc();
            }
        }

        private void OnGUI()
        {
            if (!IsOwner)
            {
                return;
            }

            Rect shortcutRect = new(16f, Screen.height - 66f, StatusSize.x, StatusSize.y);
            GUI.Label(shortcutRect, DevelopmentRuntimeOptions.StatusText);

            Rect statsRect = new(16f, Screen.height - 44f, StatusSize.x, StatusSize.y);
            GUI.Label(statsRect, cardStatsText);
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

        [ServerRpc]
        private void RequestGrantCardServerRpc(CardId card)
        {
            PlayerCardLoadout loadout = EnsureCardLoadout();
            loadout.Grant(card);
            RefreshCardStats();
            string summary = DevelopmentCardStatsText.Format(loadout.Stats, loadout.CardCount);
            SetCardStatsTextTargetRpc(Owner, summary);
            Debug.Log($"Rounds2 dev granted {CardText.NameWithEffect(card)} to {name}. {summary}");
        }

        [ServerRpc]
        private void RequestClearCardsServerRpc()
        {
            PlayerCardLoadout loadout = EnsureCardLoadout();
            loadout.Clear();
            RefreshCardStats();
            string summary = DevelopmentCardStatsText.Format(loadout.Stats, loadout.CardCount);
            SetCardStatsTextTargetRpc(Owner, summary);
            Debug.Log($"Rounds2 dev cleared cards for {name}. {summary}");
        }

        [TargetRpc]
        private void SetCardStatsTextTargetRpc(NetworkConnection connection, string summary)
        {
            cardStatsText = summary;
        }

        private PlayerCardLoadout EnsureCardLoadout()
        {
            PlayerCardLoadout loadout = GetComponent<PlayerCardLoadout>();
            if (loadout == null)
            {
                loadout = gameObject.AddComponent<PlayerCardLoadout>();
            }

            return loadout;
        }

        private void RefreshCardStats()
        {
            if (GetComponent<WeaponController>() is WeaponController weapon)
            {
                weapon.ResetAmmo();
            }

            if (GetComponent<PlayerShieldController>() is PlayerShieldController shield)
            {
                shield.ResetShield();
            }
        }
    }
}
