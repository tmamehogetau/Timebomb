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
    }
}
