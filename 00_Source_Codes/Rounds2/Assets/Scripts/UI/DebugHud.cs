using FishNet.Managing;
using UnityEngine;
using UnityEngine.UI;

namespace Rounds2.UI
{
    public sealed class DebugHud : MonoBehaviour
    {
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private Text statusText;

        private void Awake()
        {
            if (networkManager == null)
            {
                networkManager = FindFirstObjectByType<NetworkManager>();
            }
        }

        private void Update()
        {
            if (networkManager == null || statusText == null)
            {
                return;
            }

            statusText.text = DebugHudText.Format(
                networkManager.ServerManager.Started,
                networkManager.ClientManager.Started);
        }
    }
}
