using System.Collections.Generic;
using System.Reflection;
using FishNet.Object;
using NUnit.Framework;
using Rounds2.Cards;
using Rounds2.Combat;
using Rounds2.Match;
using UnityEditor;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class SetManagerDraftTests
    {
        private GameObject managerObject;
        private GameObject leftPlayerObject;
        private GameObject rightPlayerObject;

        [TearDown]
        public void TearDown()
        {
            RoundCombatGate.Open();
            Object.DestroyImmediate(managerObject);
            Object.DestroyImmediate(leftPlayerObject);
            Object.DestroyImmediate(rightPlayerObject);
        }

        [Test]
        public void SetLoserDoesNotReceiveDraftBeforeRoundLoss()
        {
            SetManager manager = CreateManager();
            Health leftPlayer = CreatePlayer("P1");
            Health rightPlayer = CreatePlayer("P2");

            RegisterPlayer(manager, leftPlayer, leftPlayerObject.transform);
            RegisterPlayer(manager, rightPlayer, rightPlayerObject.transform);

            KillPlayer(manager, leftPlayer);

            Assert.AreEqual(-1, manager.PendingDraftPlayerIndex);
            Assert.IsFalse(RoundCombatGate.IsOpen);

            PlayerCardLoadout leftLoadout = leftPlayer.GetComponent<PlayerCardLoadout>();
            PlayerCardLoadout rightLoadout = rightPlayer.GetComponent<PlayerCardLoadout>();
            Assert.AreEqual(0, leftLoadout.CardCount);
            Assert.AreEqual(0, rightLoadout.CardCount);
        }

        [Test]
        public void RoundLoserReceivesDraftAndOnlyTheyCanChooseReward()
        {
            SetManager manager = CreateManager();
            Health leftPlayer = CreatePlayer("P1");
            Health rightPlayer = CreatePlayer("P2");

            RegisterPlayer(manager, leftPlayer, leftPlayerObject.transform);
            RegisterPlayer(manager, rightPlayer, rightPlayerObject.transform);

            KillPlayer(manager, leftPlayer);
            ResetPlayersForNextSet(manager);
            KillPlayer(manager, leftPlayer);

            Assert.AreEqual(0, manager.PendingDraftPlayerIndex);
            Assert.IsFalse(RoundCombatGate.IsOpen);

            UnlockDraftChoice(manager);
            SubmitDraftChoice(manager, rightPlayer, 0);

            PlayerCardLoadout leftLoadout = leftPlayer.GetComponent<PlayerCardLoadout>();
            PlayerCardLoadout rightLoadout = rightPlayer.GetComponent<PlayerCardLoadout>();
            Assert.AreEqual(0, leftLoadout.CardCount);
            Assert.AreEqual(0, rightLoadout.CardCount);

            SubmitDraftChoice(manager, leftPlayer, 0);

            Assert.AreEqual(-1, manager.PendingDraftPlayerIndex);
            Assert.AreEqual(1, leftLoadout.CardCount);
            Assert.AreEqual(CardId.BurstShot, leftLoadout.Cards[0]);
            Assert.AreEqual(0, rightLoadout.CardCount);
        }

        private SetManager CreateManager()
        {
            managerObject = new GameObject("SetManager");
            managerObject.AddComponent<NetworkObject>();
            return managerObject.AddComponent<SetManager>();
        }

        private Health CreatePlayer(string name)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
            GameObject playerObject = Object.Instantiate(prefab);
            playerObject.name = name;
            if (leftPlayerObject == null)
            {
                leftPlayerObject = playerObject;
            }
            else
            {
                rightPlayerObject = playerObject;
            }

            return playerObject.GetComponent<Health>();
        }

        private static void UnlockDraftChoice(SetManager manager)
        {
            FieldInfo field = typeof(SetManager).GetField(
                "draftChoiceUnlockTimeSeconds",
                BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(manager, 0f);
        }

        private static void RegisterPlayer(SetManager manager, Health health, Transform spawnPoint)
        {
            Invoke(manager, "RegisterPlayerCore", health, spawnPoint);
        }

        private static void KillPlayer(SetManager manager, Health health)
        {
            Invoke(manager, "HandlePlayerDied", health);
        }

        private static void SubmitDraftChoice(SetManager manager, Health health, int choiceIndex)
        {
            Invoke(manager, "SubmitDraftChoiceCore", health, choiceIndex);
        }

        private static void ResetPlayersForNextSet(SetManager manager)
        {
            FieldInfo field = typeof(SetManager).GetField(
                "deadPlayers",
                BindingFlags.Instance | BindingFlags.NonPublic);
            List<bool> deadPlayers = (List<bool>)field.GetValue(manager);
            for (int i = 0; i < deadPlayers.Count; i++)
            {
                deadPlayers[i] = false;
            }

            RoundCombatGate.Open();
        }

        private static void Invoke(SetManager manager, string methodName, params object[] args)
        {
            InvokeWithResult(manager, methodName, args);
        }

        private static object InvokeWithResult(SetManager manager, string methodName, params object[] args)
        {
            MethodInfo method = typeof(SetManager).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            return method.Invoke(manager, args);
        }
    }
}
