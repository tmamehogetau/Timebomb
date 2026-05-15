using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using UnityEngine;

namespace Rounds2.Networking
{
    public sealed class ConnectionBootstrap : MonoBehaviour
    {
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private string onlineSceneName = "Arena01";
        [SerializeField] private bool loadOnlineSceneOnServerStart = true;

        private void Awake()
        {
            if (networkManager == null)
            {
                networkManager = FindFirstObjectByType<NetworkManager>();
            }
        }

        private void OnEnable()
        {
            if (networkManager != null)
            {
                networkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;
            }
        }

        private void OnDisable()
        {
            if (networkManager != null)
            {
                networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;
            }
        }

        private void Start()
        {
            BootstrapLaunchMode launchMode = BootstrapLaunchOptions.FromArgs(System.Environment.GetCommandLineArgs());
            Debug.Log($"Rounds2 bootstrap launch mode: {launchMode}");

            switch (launchMode)
            {
                case BootstrapLaunchMode.Server:
                    StartServer();
                    break;
                case BootstrapLaunchMode.Client:
                    StartClient();
                    break;
            }
        }

        public void StartServer()
        {
            Debug.Log("Rounds2 starting server.");
            networkManager.ServerManager.StartConnection();
        }

        public void StartClient()
        {
            Debug.Log("Rounds2 starting client.");
            networkManager.ClientManager.StartConnection();
        }

        private void OnServerConnectionState(ServerConnectionStateArgs args)
        {
            if (!loadOnlineSceneOnServerStart || args.ConnectionState != LocalConnectionState.Started)
            {
                return;
            }

            SceneLoadData sceneLoadData = new(onlineSceneName)
            {
                ReplaceScenes = ReplaceOption.All
            };
            Debug.Log($"Rounds2 loading online scene: {onlineSceneName}");
            networkManager.SceneManager.LoadGlobalScenes(sceneLoadData);
        }
    }
}
