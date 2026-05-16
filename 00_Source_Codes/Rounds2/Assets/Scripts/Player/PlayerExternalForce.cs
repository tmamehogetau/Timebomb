using System.Collections.Generic;
using UnityEngine;

namespace Rounds2.Player
{
    public readonly struct PlayerExternalForce
    {
        private const float MinimumMagnitudeSquared = 0.001f;

        private PlayerExternalForce(Vector2 initialVelocity, uint totalTicks, uint remainingTicks)
        {
            InitialVelocity = initialVelocity;
            TotalTicks = totalTicks;
            RemainingTicks = remainingTicks;
        }

        private Vector2 InitialVelocity { get; }
        private uint TotalTicks { get; }
        public Vector2 Velocity => TotalTicks > 0 ? InitialVelocity * ((float)RemainingTicks / TotalTicks) : Vector2.zero;
        public uint RemainingTicks { get; }
        public bool IsActive => RemainingTicks > 0 && Velocity.sqrMagnitude > MinimumMagnitudeSquared;

        public static PlayerExternalForce Create(Vector2 direction, float speed, uint durationTicks)
        {
            if (direction.sqrMagnitude <= MinimumMagnitudeSquared || speed <= 0f || durationTicks == 0)
            {
                return default;
            }

            return new PlayerExternalForce(direction.normalized * speed, durationTicks, durationTicks);
        }

        public PlayerExternalForce ConsumeTick()
        {
            if (!IsActive || RemainingTicks == 1)
            {
                return default;
            }

            return new PlayerExternalForce(InitialVelocity, TotalTicks, RemainingTicks - 1);
        }
    }

    public sealed class PlayerExternalForceQueue
    {
        private readonly List<PlayerExternalForce> forces = new();

        public void Enqueue(PlayerExternalForce force)
        {
            if (force.IsActive)
            {
                forces.Add(force);
            }
        }

        public Vector2 ConsumeTickVelocity()
        {
            Vector2 velocity = Vector2.zero;

            for (int i = forces.Count - 1; i >= 0; i--)
            {
                PlayerExternalForce force = forces[i];
                velocity += force.Velocity;

                PlayerExternalForce next = force.ConsumeTick();
                if (next.IsActive)
                {
                    forces[i] = next;
                }
                else
                {
                    forces.RemoveAt(i);
                }
            }

            return velocity;
        }

        public void Clear()
        {
            forces.Clear();
        }
    }
}
