using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class PredictedPlayerMotorSourceTests
    {
        [Test]
        public void PredictionDataUsesFishNetReplicateAndReconcileInterfaces()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Player", "PlayerPredictionData.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("IReplicateData", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("IReconcileData", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("PredictionRigidbody2D", StringComparison.Ordinal));
        }

        [Test]
        public void PredictedMotorRunsReplicateAndReconcileOnFishNetTicks()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Player", "PredictedPlayerMotor.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("TickNetworkBehaviour", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("SetTickCallbacks", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("TimeManager_OnTick", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("TimeManager_OnPostTick", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("[Replicate]", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("[Reconcile]", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("PredictionRigidbody2D", StringComparison.Ordinal));
        }

        [Test]
        public void PredictedMotorUsesVelocityAndOwnsAimIndicatorVisuals()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Player", "PredictedPlayerMotor.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("predictedBody.Velocity", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("predictedBody.MovePosition", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("predictedBody.MoveRotation", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("AimIndicator", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("ApplyAimIndicator", StringComparison.Ordinal));
        }

        [Test]
        public void PredictedMotorReadsHumanMoveInputInsideNetworkTick()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Player", "PredictedPlayerMotor.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("ReadMoveInput(Keyboard.current)", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("DevelopmentRuntimeOptions.BotEnabled", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("return new PlayerReplicateData(move, aimDirection, fire: false)", StringComparison.Ordinal));
        }

        [Test]
        public void PredictedMotorBlocksMoveInputWhenRoundCombatGateIsClosed()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Player", "PredictedPlayerMotor.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("RoundCombatGate.IsOpen", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("Vector2 move = RoundCombatGate.IsOpen", StringComparison.Ordinal));
        }

        [Test]
        public void PlayerInputReaderFeedsPredictedMotorWithoutOwningMovementSimulation()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Player", "PlayerInputReader.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("PredictedPlayerMotor", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("SubmitOwnerInput", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("Rigidbody2D", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("ServerRpc", StringComparison.Ordinal));
        }

        [Test]
        public void PlayerControllerDelegatesToPredictedMotor()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Player", "PlayerController.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("PredictedPlayerMotor", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("SubmitInputServerRpc", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("FixedUpdate", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("PlayerSeparationRules.FilterVelocity", StringComparison.Ordinal));
        }

        [Test]
        public void ProjectSetupConfiguresPredictionManagerAndTimeManagerPhysics()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Editor", "Rounds2ProjectSetup.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("PredictionManager", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("AddComponent<TimeManager>", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("_physicsMode", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("PhysicsMode.TimeManager", StringComparison.Ordinal));
        }

        [Test]
        public void RuntimeBootstrapForcesTimeManagerPhysics()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Networking", "ConnectionBootstrap.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("SetTickRate(NetworkTuning.TickRate)", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("SetPhysicsMode(PhysicsMode.TimeManager)", StringComparison.Ordinal));
        }

        [Test]
        public void PredictedMotorCombinesExternalForceInsideReplicate()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Player", "PredictedPlayerMotor.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("PlayerExternalForceQueue", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("movementVelocity", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("PlayerMotion.SmoothMoveVelocity", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("ConsumeTickVelocity()", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("movementVelocity + externalVelocity", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("ApplyExternalForceTargetRpc", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("ApplyExternalForceObserversRpc", StringComparison.Ordinal));
        }

        [Test]
        public void PredictionDataReconcilesSmoothedMoveVelocity()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Player", "PlayerPredictionData.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("public Vector2 MoveVelocity", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("Vector2 moveVelocity", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("MoveVelocity = moveVelocity", StringComparison.Ordinal));
        }

        [Test]
        public void PredictedMotorReconcilesMovementInertiaState()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Player", "PredictedPlayerMotor.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("new(predictedBody, aimDirection, movementVelocity)", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("movementVelocity = data.MoveVelocity", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("movementVelocity = Vector2.zero", StringComparison.Ordinal));
        }

        [Test]
        public void PredictedMotorSynchronizesAimForRemoteAimIndicators()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Player", "PredictedPlayerMotor.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("SyncVar<Vector2> syncedAimDirection", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("syncedAimDirection.UpdateSendRate(0f)", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("syncedAimDirection.OnChange +=", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("OnSyncedAimDirectionChanged", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("SyncAimDirectionServerRpc", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("syncedAimDirection.Value = normalizedAim", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("SyncAimDirectionObserversRpc", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("lastSentAimDirection", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("AimSyncIntervalSeconds", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("lastAimSyncTimeSeconds", StringComparison.Ordinal));
        }

        [Test]
        public void PredictedMotorSmoothsRemoteAimIndicatorLocally()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Player", "PredictedPlayerMotor.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("displayAimDirection", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("targetAimDirection", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("AimIndicatorFollowSpeed", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("UpdateRemoteAimIndicator", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("Vector2.Lerp(displayAimDirection, targetAimDirection", StringComparison.Ordinal));
        }

        [Test]
        public void PredictedMotorOwnsSubtleFireFeedback()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Player", "PredictedPlayerMotor.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("MuzzleFlash", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("PlayFireFeedback", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("CombatTuning.FireFeedbackRecoilDistance", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("CombatTuning.FireFeedbackRecoilSeconds", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("CombatTuning.MuzzleFlashSeconds", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("CombatTuning.MuzzleFlashScale", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("ApplyMuzzleFlash", StringComparison.Ordinal));
        }

        [Test]
        public void PredictedMotorDoesNotOverwriteRemoteAimFromPredictionTicks()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Player", "PredictedPlayerMotor.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("private bool ShouldApplyPredictedAim() => IsOwner || IsServerInitialized", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("if (ShouldApplyPredictedAim())", StringComparison.Ordinal));
            Assert.GreaterOrEqual(CountOccurrences(source, "if (ShouldApplyPredictedAim())"), 2);
        }

        [Test]
        public void BulletHitAppliesPredictedKnockbackToTargets()
        {
            string sourcePath = Path.Combine(Application.dataPath, "Scripts", "Combat", "Bullet.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.IsTrue(source.Contains("PredictedPlayerMotor", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("ApplyExternalForce", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("CombatTuning.BulletKnockbackSpeed", StringComparison.Ordinal));
            Assert.IsTrue(source.Contains("CombatTuning.BulletKnockbackDurationTicks", StringComparison.Ordinal));
        }

        private static int CountOccurrences(string source, string value)
        {
            int count = 0;
            int index = 0;
            while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += value.Length;
            }

            return count;
        }
    }
}
