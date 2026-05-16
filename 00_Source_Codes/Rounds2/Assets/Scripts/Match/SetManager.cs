using FishNet.Object;
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

        private readonly List<Health> players = new();
        private readonly List<bool> deadPlayers = new();
        private readonly List<PlayerRoundEntry> playerEntries = new();
        private readonly RoundScoreState score = new();
        private Coroutine resetRoundCoroutine;

        public Health Winner { get; private set; }

        [Server]
        public void RegisterPlayer(Health health)
        {
            RegisterPlayer(health, health != null ? health.transform : null);
        }

        [Server]
        public void RegisterPlayer(Health health, Transform spawnPoint)
        {
            if (health == null || players.Contains(health))
            {
                return;
            }

            health.ResetHealth();
            players.Add(health);
            deadPlayers.Add(health.IsDead);
            playerEntries.Add(new PlayerRoundEntry(health, spawnPoint));
            score.AddPlayer();
            health.Changed += OnHealthChanged;
            health.Died += OnPlayerDied;
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
        private void OnPlayerDied(Health deadPlayer)
        {
            if (score.IsMatchFinished || resetRoundCoroutine != null)
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
                score.RecordRoundWin(winnerIndex, MatchTuning.RoundWinsToWinMatch);
                Debug.Log($"Round winner: {Winner.name} ({score.GetRoundWins(winnerIndex)}/{MatchTuning.RoundWinsToWinMatch})");
                UpdateScoreHud();

                if (score.IsMatchFinished)
                {
                    Debug.Log($"Match winner: {Winner.name}");
                    return;
                }

                resetRoundCoroutine = StartCoroutine(ResetRoundAfterDelay());
            }
        }

        [Server]
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
            UpdateHealthHud();
        }

        [Server]
        public void ResetMatch()
        {
            if (resetRoundCoroutine != null)
            {
                StopCoroutine(resetRoundCoroutine);
                resetRoundCoroutine = null;
            }

            DespawnActiveBullets();
            score.ResetMatch();

            for (int i = 0; i < playerEntries.Count; i++)
            {
                deadPlayers[i] = false;
                playerEntries[i].ResetForNextRound();
            }

            Winner = null;
            RoundCombatGate.Open();
            UpdateScoreHud();
            UpdateHealthHud();
            Debug.Log("Match reset by development shortcut.");
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
            string winnerLabel = score.MatchWinnerIndex >= 0 ? $"P{score.MatchWinnerIndex + 1}" : string.Empty;
            string text = ScoreHudText.Format(
                leftWins,
                rightWins,
                MatchTuning.RoundWinsToWinMatch,
                score.IsMatchFinished,
                winnerLabel);

            if (IsSpawned)
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
            string text = HealthHudText.Format(leftHealth, rightHealth, CombatTuning.BaseHealth);

            if (IsSpawned)
            {
                SetHealthHudObserversRpc(text);
            }
            else
            {
                HealthHud.SetHealthText(text);
            }
        }

        [Server]
        private void DespawnActiveBullets()
        {
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
            private readonly Vector3 spawnPosition;
            private readonly Quaternion spawnRotation;

            public Vector3 SpawnPosition => spawnPosition;

            public PlayerRoundEntry(Health health, Transform spawnPoint)
            {
                this.health = health;
                body = health.GetComponent<Rigidbody2D>();
                controller = health.GetComponent<PlayerController>();
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

                health.ResetHealth();
            }
        }
    }
}
