using FishNet.Object;
using Rounds2.Config;
using System;

namespace Rounds2.Combat
{
    public sealed class Health : NetworkBehaviour
    {
        private readonly HealthState state = new();
        private HealthVisuals visuals;

        public event Action<Health> Changed;
        public event Action<Health> Died;

        public int Current => state.Current;
        public bool IsDead => state.IsDead;

        private void Awake()
        {
            visuals = GetComponent<HealthVisuals>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            ResetHealth();
        }

        [Server]
        public void ResetHealth()
        {
            state.Reset(CombatTuning.BaseHealth);
            SetAliveVisualState(true);
            Changed?.Invoke(this);

            if (IsSpawned)
            {
                SetAliveVisualStateObserversRpc(true);
            }
        }

        [Server]
        public void ApplyDamage(int amount)
        {
            if (amount <= 0 || IsDead)
            {
                return;
            }

            if (state.ApplyDamage(amount))
            {
                Changed?.Invoke(this);

                if (IsSpawned)
                {
                    SetAliveVisualStateObserversRpc(false);
                }
                else
                {
                    SetAliveVisualState(false);
                }

                UnityEngine.Debug.Log($"Rounds2 {name} took {amount} damage and died at {FormatPosition(transform.position)}.");
                Died?.Invoke(this);
                return;
            }

            Changed?.Invoke(this);
            UnityEngine.Debug.Log($"Rounds2 {name} took {amount} damage. HP: {Current}. pos={FormatPosition(transform.position)}");
        }

        [ObserversRpc(BufferLast = true, RunLocally = true)]
        private void SetAliveVisualStateObserversRpc(bool alive)
        {
            SetAliveVisualState(alive);
        }

        private void SetAliveVisualState(bool alive)
        {
            if (visuals == null)
            {
                visuals = GetComponent<HealthVisuals>();
            }

            visuals?.SetAlive(alive);
        }

        private static string FormatPosition(UnityEngine.Vector3 position)
        {
            return $"({position.x:0.00},{position.y:0.00})";
        }
    }
}
