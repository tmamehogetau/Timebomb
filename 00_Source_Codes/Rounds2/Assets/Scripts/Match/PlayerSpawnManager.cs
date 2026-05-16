using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
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
            if (networkManager == null)
            {
                networkManager = FindFirstObjectByType<NetworkManager>();
            }

            if (networkManager != null)
            {
                networkManager.SceneManager.OnClientLoadedStartScenes += OnClientLoadedStartScenes;
            }
        }

        private void OnDisable()
        {
            if (networkManager != null)
            {
                networkManager.SceneManager.OnClientLoadedStartScenes -= OnClientLoadedStartScenes;
            }
        }

        private void OnClientLoadedStartScenes(NetworkConnection connection, bool asServer)
        {
            if (!asServer || playerPrefab == null)
            {
                return;
            }

            Transform spawnPoint = GetNextSpawnPoint();
            NetworkObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            networkManager.ServerManager.Spawn(player, connection);
            Debug.Log($"Rounds2 spawned player for connection {connection.ClientId} at {spawnPoint.position}.");

            Health health = player.GetComponent<Health>();
            if (health != null && setManager != null)
            {
                setManager.RegisterPlayer(health, spawnPoint);
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
