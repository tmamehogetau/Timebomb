using FishNet.Object;
using Rounds2.Combat;
using Rounds2.Development;
using Rounds2.Match;
using Rounds2.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rounds2.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PredictedPlayerMotor))]
    public sealed class PlayerController : NetworkBehaviour
    {
        private PredictedPlayerMotor motor;
        private Health health;

        public Vector2 AimDirection => motor != null ? motor.AimDirection : Vector2.right;

        private void Awake()
        {
            motor = GetComponent<PredictedPlayerMotor>();
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            DraftHud.ChoiceClicked += SubmitDraftChoice;
        }

        private void OnDisable()
        {
            DraftHud.ChoiceClicked -= SubmitDraftChoice;
        }

        private void Update()
        {
            if (!IsOwner || DevelopmentRuntimeOptions.BotEnabled || Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                SubmitDraftChoice(0);
            }
            else if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                SubmitDraftChoice(1);
            }
            else if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                SubmitDraftChoice(2);
            }
        }

        public void SubmitOwnerInput(Vector2 move, Vector2 aim)
        {
            motor?.SubmitOwnerInput(move, aim);
        }

        public void SubmitDraftChoice(int choiceIndex)
        {
            if (!IsOwner)
            {
                return;
            }

            SubmitDraftChoiceServerRpc(choiceIndex);
        }

        [Server]
        public void ResetRoundTransform(Vector3 position, Quaternion rotation)
        {
            if (motor != null)
            {
                motor.ResetRoundTransform(position, rotation);
            }
        }

        [ServerRpc]
        private void SubmitDraftChoiceServerRpc(int choiceIndex)
        {
            health ??= GetComponent<Health>();
            SetManager setManager = FindFirstObjectByType<SetManager>();
            if (setManager != null)
            {
                setManager.SubmitDraftChoice(health, choiceIndex);
            }
        }
    }
}
