using FishNet.Object;
using Rounds2.Combat;
using Rounds2.Development;
using UnityEngine;

namespace Rounds2.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(WeaponController))]
    public sealed class PlayerBotController : NetworkBehaviour
    {
        private PlayerController player;
        private WeaponController weapon;
        private bool loggedEnabled;
        private float nextAllowedFireTimeSeconds;

        private const float InitialFireDelaySeconds = 2f;
        private const float FireIntervalSeconds = 2.4f;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            weapon = GetComponent<WeaponController>();
        }

        private void Update()
        {
            if (!DevelopmentRuntimeOptions.BotEnabled || !IsOwner)
            {
                loggedEnabled = false;
                return;
            }

            if (!loggedEnabled)
            {
                loggedEnabled = true;
                nextAllowedFireTimeSeconds = Time.time + InitialFireDelaySeconds;
                Debug.Log($"Rounds2 bot enabled for {name}.");
            }

            if (!TryFindTarget(out Vector2 targetPosition))
            {
                player.SubmitOwnerInput(Vector2.zero, player.AimDirection);
                return;
            }

            PlayerBotCommand command = PlayerBotInput.Decide(transform.position, targetPosition, Time.time, nextAllowedFireTimeSeconds);
            player.SubmitOwnerInput(command.Move, command.Aim);

            if (command.ShouldFire)
            {
                weapon.TryFire(command.Aim);
                nextAllowedFireTimeSeconds = Time.time + FireIntervalSeconds;
            }
        }

        private bool TryFindTarget(out Vector2 targetPosition)
        {
            Health[] candidates = FindObjectsByType<Health>(FindObjectsSortMode.None);
            float bestDistanceSqr = float.MaxValue;
            Health bestTarget = null;

            foreach (Health candidate in candidates)
            {
                if (candidate == null || candidate.NetworkObject == NetworkObject)
                {
                    continue;
                }

                float distanceSqr = ((Vector2)candidate.transform.position - (Vector2)transform.position).sqrMagnitude;
                if (distanceSqr < bestDistanceSqr)
                {
                    bestDistanceSqr = distanceSqr;
                    bestTarget = candidate;
                }
            }

            if (bestTarget == null)
            {
                targetPosition = Vector2.zero;
                return false;
            }

            targetPosition = bestTarget.transform.position;
            return true;
        }
    }
}
