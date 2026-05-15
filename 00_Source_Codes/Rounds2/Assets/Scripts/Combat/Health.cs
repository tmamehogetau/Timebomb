using FishNet.Object;
using Rounds2.Config;
using System;

namespace Rounds2.Combat
{
    public sealed class Health : NetworkBehaviour
    {
        private readonly HealthState state = new();

        public event Action<Health> Died;

        public int Current => state.Current;
        public bool IsDead => state.IsDead;

        public override void OnStartServer()
        {
            base.OnStartServer();
            ResetHealth();
        }

        [Server]
        public void ResetHealth()
        {
            state.Reset(CombatTuning.BaseHealth);
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
                Died?.Invoke(this);
            }
        }
    }
}
