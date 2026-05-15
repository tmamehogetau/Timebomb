using FishNet.Object;
using Rounds2.Combat;
using System.Collections.Generic;
using UnityEngine;

namespace Rounds2.Match
{
    public sealed class SetManager : NetworkBehaviour
    {
        private readonly List<Health> players = new();
        private readonly List<bool> deadPlayers = new();

        public Health Winner { get; private set; }

        [Server]
        public void RegisterPlayer(Health health)
        {
            if (health == null || players.Contains(health))
            {
                return;
            }

            players.Add(health);
            deadPlayers.Add(health.IsDead);
            health.Died += OnPlayerDied;
        }

        [Server]
        private void OnPlayerDied(Health deadPlayer)
        {
            int deadIndex = players.IndexOf(deadPlayer);
            if (deadIndex >= 0)
            {
                deadPlayers[deadIndex] = true;
            }

            if (SetRules.TryFindWinner(deadPlayers, out int winnerIndex))
            {
                Winner = players[winnerIndex];
                Debug.Log($"Set winner: {Winner.name}");
            }
        }
    }
}
