using FishNet.Managing;
using UnityEngine;

namespace Rounds2.Networking
{
    public sealed class ConnectionBootstrap : MonoBehaviour
    {
        [SerializeField] private NetworkManager networkManager;

        private void Awake()
        {
            if (networkManager == null)
            {
                networkManager = FindFirstObjectByType<NetworkManager>();
            }
        }

        private void Start()
        {
            foreach (string arg in System.Environment.GetCommandLineArgs())
            {
                if (arg == "-server")
                {
                    StartServer();
                    return;
                }

                if (arg == "-client")
                {
                    StartClient();
                    return;
                }
            }
        }

        public void StartServer()
        {
            networkManager.ServerManager.StartConnection();
        }

        public void StartClient()
        {
            networkManager.ClientManager.StartConnection();
        }
    }
}
