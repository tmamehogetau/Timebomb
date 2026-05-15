using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;
using Rounds2.Combat;
using UnityEngine;

namespace Rounds2.Match
{
    public sealed class PlayerSpawnManager : MonoBehaviour
    {
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private NetworkObject playerPrefab;
        [SerializeField] private SetManager setManager;
        [SerializeField] private Transform[] spawnPoints;

        private int nextSpawnIndex;

        private void Awake()
        {
            if (networkManager == null)
            {
                networkManager = FindFirstObjectByType<NetworkManager>();
            }

            if (setManager == null)
            {
                setManager = FindFirstObjectByType<SetManager>();
            }
        }

        private void OnEnable()
        {
            if (networkManager != null)
            {
                networkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
            }
        }

        private void OnDisable()
        {
            if (networkManager != null)
            {
                networkManager.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
            }
        }

        private void OnRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs args)
        {
            if (args.ConnectionState != RemoteConnectionState.Started || playerPrefab == null)
            {
                return;
            }

            Transform spawnPoint = GetNextSpawnPoint();
            NetworkObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            networkManager.ServerManager.Spawn(player, connection);

            Health health = player.GetComponent<Health>();
            if (health != null && setManager != null)
            {
                setManager.RegisterPlayer(health);
            }
        }

        private Transform GetNextSpawnPoint()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                return transform;
            }

            Transform spawnPoint = spawnPoints[nextSpawnIndex % spawnPoints.Length];
            nextSpawnIndex++;
            return spawnPoint;
        }
    }
}
