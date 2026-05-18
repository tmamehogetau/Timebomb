using FishNet.Object;
using Rounds2.Cards;
using Rounds2.Combat;
using Rounds2.Config;
using Rounds2.Player;
using Rounds2.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rounds2.Match
{
    public sealed class SetManager : NetworkBehaviour
    {
        [SerializeField] private float roundResetDelaySeconds = 1.5f;

        private List<Health> players = new();
        private List<bool> deadPlayers = new();
        private List<PlayerRoundEntry> playerEntries = new();
        private RoundScoreState score = new();
        private Coroutine resetRoundCoroutine;
        private string latestRewardText = string.Empty;
        private string rewardHudText = string.Empty;
        private int pendingDraftPlayerIndex = -1;
        private CardDraftOffer pendingDraftOffer;
        private float draftChoiceUnlockTimeSeconds;
        private bool networkServerStarted;

        private const float MinimumDraftDisplaySeconds = 1.5f;

        public Health Winner { get; private set; }
        public int PendingDraftPlayerIndex => pendingDraftPlayerIndex;

        private void Awake()
        {
            EnsureState();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            networkServerStarted = true;
        }

        [Server]
        public void RegisterPlayer(Health health)
        {
            RegisterPlayer(health, health != null ? health.transform : null);
        }

        [Server]
        public void RegisterPlayer(Health health, Transform spawnPoint)
        {
            RegisterPlayerCore(health, spawnPoint);
        }

        private void RegisterPlayerCore(Health health, Transform spawnPoint)
        {
            EnsureState();
            if (health == null || players.Contains(health))
            {
                return;
            }

            health.ResetHealthCore(syncObservers: false);
            EnsureCardLoadout(health);
            players.Add(health);
            deadPlayers.Add(health.IsDead);
            playerEntries.Add(new PlayerRoundEntry(health, spawnPoint));
            score.AddPlayer();
            health.Changed += OnHealthChanged;
            health.Died += OnPlayerDied;
            WeaponController weapon = health.GetComponent<WeaponController>();
            if (weapon != null)
            {
                weapon.AmmoChanged += OnWeaponAmmoChanged;
            }

            PlayerShieldController shield = health.GetComponent<PlayerShieldController>();
            if (shield != null)
            {
                shield.ShieldChanged += OnShieldChanged;
            }

            Debug.Log($"Rounds2 registered player {health.name} with HP {health.Current}.");
            UpdateScoreHud();
            UpdateHealthHud();
        }

        [Server]
        private void OnHealthChanged(Health health)
        {
            UpdateHealthHud();
        }

        [Server]
        private void OnWeaponAmmoChanged(WeaponController weapon)
        {
            UpdateHealthHud();
        }

        [Server]
        private void OnShieldChanged(PlayerShieldController shield)
        {
            UpdateHealthHud();
        }

        [Server]
        private void OnPlayerDied(Health deadPlayer)
        {
            HandlePlayerDied(deadPlayer);
        }

        private void HandlePlayerDied(Health deadPlayer)
        {
            EnsureState();
            if (score.IsMatchFinished || resetRoundCoroutine != null || pendingDraftPlayerIndex >= 0)
            {
                return;
            }

            int deadIndex = players.IndexOf(deadPlayer);
            if (deadIndex >= 0)
            {
                deadPlayers[deadIndex] = true;
            }

            if (SetRules.TryFindWinner(deadPlayers, out int winnerIndex))
            {
                RoundCombatGate.Close();
                DespawnActiveBullets();
                Winner = players[winnerIndex];
                bool roundFinished = score.RecordSetWin(
                    winnerIndex,
                    MatchTuning.SetWinsToWinRound,
                    MatchTuning.RoundWinsToWinMatch);
                string resultLabel = roundFinished ? "Round winner" : "Set winner";
                Debug.Log($"{resultLabel}: {Winner.name} (sets {score.GetSetWins(winnerIndex)}/{MatchTuning.SetWinsToWinRound}, rounds {score.GetRoundWins(winnerIndex)}/{MatchTuning.RoundWinsToWinMatch})");

                if (score.IsMatchFinished)
                {
                    UpdateScoreHud();
                    Debug.Log($"Match winner: {Winner.name}");
                    return;
                }

                if (roundFinished)
                {
                    BeginDraft(deadIndex);
                }
                else
                {
                    ScheduleRoundReset();
                }

                UpdateScoreHud();
            }
        }

        private IEnumerator ResetRoundAfterDelay()
        {
            yield return new WaitForSeconds(roundResetDelaySeconds);
            DespawnActiveBullets();

            for (int i = 0; i < playerEntries.Count; i++)
            {
                deadPlayers[i] = false;
                playerEntries[i].ResetForNextRound();
                Debug.Log($"Round reset player {i + 1} to {playerEntries[i].SpawnPosition}.");
            }

            Winner = null;
            resetRoundCoroutine = null;
            RoundCombatGate.Open();
            Debug.Log("Round reset.");
            rewardHudText = string.Empty;
            UpdateDraftHud();
            UpdateHealthHud();
        }

        [Server]
        public void ResetMatch()
        {
            EnsureState();
            if (resetRoundCoroutine != null)
            {
                StopCoroutine(resetRoundCoroutine);
                resetRoundCoroutine = null;
            }

            DespawnActiveBullets();
            score.ResetMatch();
            latestRewardText = string.Empty;
            rewardHudText = string.Empty;
            pendingDraftPlayerIndex = -1;
            draftChoiceUnlockTimeSeconds = 0f;

            for (int i = 0; i < playerEntries.Count; i++)
            {
                deadPlayers[i] = false;
                EnsureCardLoadout(players[i]).Clear();
                playerEntries[i].ResetForNextRound();
            }

            Winner = null;
            RoundCombatGate.Open();
            UpdateScoreHud();
            UpdateHealthHud();
            Debug.Log("Match reset by development shortcut.");
        }

        [Server]
        public void SubmitDraftChoice(Health player, int choiceIndex)
        {
            SubmitDraftChoiceCore(player, choiceIndex);
        }

        private void SubmitDraftChoiceCore(Health player, int choiceIndex)
        {
            EnsureState();
            if (player == null
                || pendingDraftPlayerIndex < 0
                || pendingDraftPlayerIndex >= players.Count
                || players[pendingDraftPlayerIndex] != player
                || Time.time < draftChoiceUnlockTimeSeconds
                || choiceIndex < 0
                || choiceIndex >= CardDraftOffer.Count)
            {
                return;
            }

            int playerIndex = pendingDraftPlayerIndex;
            CardId reward = GrantDraftReward(playerIndex, choiceIndex);
            rewardHudText = DraftHudText.FormatReward(
                playerIndex + 1,
                reward,
                EnsureCardLoadout(players[playerIndex]).Cards);
            pendingDraftPlayerIndex = -1;
            draftChoiceUnlockTimeSeconds = 0f;
            UpdateDraftHud();
            UpdateScoreHud();
            ScheduleRoundReset();
        }

        private void ScheduleRoundReset()
        {
            if (!networkServerStarted)
            {
                return;
            }

            resetRoundCoroutine = StartCoroutine(ResetRoundAfterDelay());
        }

        private void BeginDraft(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= players.Count)
            {
                return;
            }

            PlayerCardLoadout loadout = EnsureCardLoadout(players[playerIndex]);
            rewardHudText = string.Empty;
            pendingDraftPlayerIndex = playerIndex;
            pendingDraftOffer = CardDraftOffer.Create(loadout.CardCount);
            draftChoiceUnlockTimeSeconds = Time.time + MinimumDraftDisplaySeconds;
            latestRewardText = $"P{playerIndex + 1} choose: {pendingDraftOffer.FormatChoices()}";
            Debug.Log($"Card draft: {latestRewardText}.");
            UpdateDraftHud();
        }

        private CardId GrantDraftReward(int playerIndex, int choiceIndex)
        {
            PlayerCardLoadout loadout = EnsureCardLoadout(players[playerIndex]);
            CardId reward = pendingDraftOffer.GetChoice(choiceIndex);
            loadout.Grant(reward);
            latestRewardText = $"P{playerIndex + 1} gained {CardText.NameWithEffect(reward)}";
            Debug.Log($"Card reward: {latestRewardText}.");
            return reward;
        }

        private static PlayerCardLoadout EnsureCardLoadout(Health health)
        {
            PlayerCardLoadout loadout = health.GetComponent<PlayerCardLoadout>();
            if (loadout == null)
            {
                loadout = health.gameObject.AddComponent<PlayerCardLoadout>();
            }

            return loadout;
        }

        private void EnsureState()
        {
            if (players == null)
            {
                players = new List<Health>();
            }

            if (deadPlayers == null)
            {
                deadPlayers = new List<bool>();
            }

            if (playerEntries == null)
            {
                playerEntries = new List<PlayerRoundEntry>();
            }

            if (score == null)
            {
                score = new RoundScoreState();
            }
        }

        [ObserversRpc(BufferLast = true, RunLocally = true)]
        private void SetDraftHudObserversRpc(string text)
        {
            DraftHud.SetDraftText(text);
        }

        private void UpdateDraftHud()
        {
            string text = pendingDraftPlayerIndex >= 0
                ? DraftHudText.FormatChoiceOffer(
                    pendingDraftPlayerIndex + 1,
                    pendingDraftOffer,
                    EnsureCardLoadout(players[pendingDraftPlayerIndex]).Cards)
                : rewardHudText;

            if (ShouldSyncObservers())
            {
                SetDraftHudObserversRpc(text);
            }
            else
            {
                DraftHud.SetDraftText(text);
            }
        }

        [ObserversRpc(BufferLast = true, RunLocally = true)]
        private void SetScoreHudObserversRpc(string text)
        {
            ScoreHud.SetScoreText(text);
        }

        private void UpdateScoreHud()
        {
            int leftWins = score.GetRoundWins(0);
            int rightWins = score.GetRoundWins(1);
            int leftSetWins = score.GetSetWins(0);
            int rightSetWins = score.GetSetWins(1);
            string winnerLabel = score.MatchWinnerIndex >= 0 ? $"P{score.MatchWinnerIndex + 1}" : string.Empty;
            string text = ScoreHudText.Format(
                leftWins,
                rightWins,
                leftSetWins,
                rightSetWins,
                MatchTuning.SetWinsToWinRound,
                MatchTuning.RoundWinsToWinMatch,
                score.IsMatchFinished,
                winnerLabel,
                latestRewardText);

            if (ShouldSyncObservers())
            {
                SetScoreHudObserversRpc(text);
            }
            else
            {
                ScoreHud.SetScoreText(text);
            }
        }

        [ObserversRpc(BufferLast = true, RunLocally = true)]
        private void SetHealthHudObserversRpc(string text)
        {
            HealthHud.SetHealthText(text);
        }

        private void UpdateHealthHud()
        {
            int leftHealth = players.Count > 0 ? players[0].Current : CombatTuning.BaseHealth;
            int rightHealth = players.Count > 1 ? players[1].Current : CombatTuning.BaseHealth;
            WeaponController leftWeapon = GetWeapon(0);
            WeaponController rightWeapon = GetWeapon(1);
            PlayerShieldController leftShield = GetShield(0);
            PlayerShieldController rightShield = GetShield(1);
            string text = HealthHudText.Format(
                leftHealth,
                rightHealth,
                CombatTuning.BaseHealth,
                GetCurrentAmmo(leftWeapon),
                GetCurrentAmmo(rightWeapon),
                GetMagazineSize(leftWeapon),
                GetMagazineSize(rightWeapon),
                leftWeapon != null && leftWeapon.IsReloading,
                rightWeapon != null && rightWeapon.IsReloading,
                GetShieldStatus(leftShield),
                GetShieldStatus(rightShield));

            if (ShouldSyncObservers())
            {
                SetHealthHudObserversRpc(text);
            }
            else
            {
                HealthHud.SetHealthText(text);
            }
        }

        private WeaponController GetWeapon(int playerIndex)
        {
            return playerIndex < players.Count ? players[playerIndex].GetComponent<WeaponController>() : null;
        }

        private PlayerShieldController GetShield(int playerIndex)
        {
            return playerIndex < players.Count ? players[playerIndex].GetComponent<PlayerShieldController>() : null;
        }

        private static int GetCurrentAmmo(WeaponController weapon)
        {
            return weapon != null ? weapon.CurrentAmmo : CombatTuning.MagazineSize;
        }

        private static int GetMagazineSize(WeaponController weapon)
        {
            return weapon != null ? weapon.MagazineSize : CombatTuning.MagazineSize;
        }

        private static string GetShieldStatus(PlayerShieldController shield)
        {
            return shield != null ? shield.StatusLabel : "Ready";
        }

        private bool ShouldSyncObservers()
        {
            return networkServerStarted && IsSpawned;
        }

        private void DespawnActiveBullets()
        {
            if (!networkServerStarted)
            {
                return;
            }

            Bullet[] bullets = FindObjectsByType<Bullet>(FindObjectsSortMode.None);
            foreach (Bullet bullet in bullets)
            {
                if (bullet != null && bullet.IsSpawned)
                {
                    bullet.Despawn();
                }
            }
        }

        private sealed class PlayerRoundEntry
        {
            private readonly Health health;
            private readonly Rigidbody2D body;
            private readonly PlayerController controller;
            private readonly WeaponController weapon;
            private readonly PlayerShieldController shield;
            private readonly Vector3 spawnPosition;
            private readonly Quaternion spawnRotation;

            public Vector3 SpawnPosition => spawnPosition;

            public PlayerRoundEntry(Health health, Transform spawnPoint)
            {
                this.health = health;
                body = health.GetComponent<Rigidbody2D>();
                controller = health.GetComponent<PlayerController>();
                weapon = health.GetComponent<WeaponController>();
                shield = health.GetComponent<PlayerShieldController>();
                Transform source = spawnPoint != null ? spawnPoint : health.transform;
                spawnPosition = source.position;
                spawnRotation = source.rotation;
            }

            public void ResetForNextRound()
            {
                if (controller != null)
                {
                    controller.ResetRoundTransform(spawnPosition, spawnRotation);
                }
                else
                {
                    PlayerRoundReset.Apply(health.transform, body, spawnPosition, spawnRotation);
                }

                weapon?.ResetAmmo();
                shield?.ResetShield();
                health.ResetHealthCore();
            }
        }
    }
}
