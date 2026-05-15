using FishNet.Managing;
using Rounds2.Networking;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Rounds2.Editor
{
    public static class Rounds2ProjectSetup
    {
        private static readonly string[] NetworkManagerPrefabPaths =
        {
            "Packages/com.firstgeargames.fishnet/Demos/Prefabs/NetworkManager.prefab",
            "Packages/FishNet: Networking Evolved/Demos/Prefabs/NetworkManager.prefab"
        };

        public static void CreateBootstrapScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Bootstrap";

            NetworkManager networkManager = CreateNetworkManager();
            ConnectionBootstrap bootstrap = CreateBootstrap(networkManager);
            CreateEventSystem();
            CreateCanvas(bootstrap);

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Bootstrap.unity");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static NetworkManager CreateNetworkManager()
        {
            foreach (string path in NetworkManagerPrefabPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.name = "NetworkManager";
                return instance.GetComponent<NetworkManager>();
            }

            GameObject fallback = new("NetworkManager");
            return fallback.AddComponent<NetworkManager>();
        }

        private static ConnectionBootstrap CreateBootstrap(NetworkManager networkManager)
        {
            GameObject bootstrapObject = new("ConnectionBootstrap");
            ConnectionBootstrap bootstrap = bootstrapObject.AddComponent<ConnectionBootstrap>();
            SerializedObject serializedBootstrap = new(bootstrap);
            serializedBootstrap.FindProperty("networkManager").objectReferenceValue = networkManager;
            serializedBootstrap.ApplyModifiedPropertiesWithoutUndo();
            return bootstrap;
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystemObject = new("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private static void CreateCanvas(ConnectionBootstrap bootstrap)
        {
            GameObject canvasObject = new("Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            CreateButton(canvas.transform, "Start Server", new Vector2(0f, 40f), bootstrap.StartServer);
            CreateButton(canvas.transform, "Start Client", new Vector2(0f, -20f), bootstrap.StartClient);
        }

        private static void CreateButton(Transform parent, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonObject = new(label);
            buttonObject.transform.SetParent(parent, false);

            RectTransform buttonRect = buttonObject.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(180f, 44f);
            buttonRect.anchoredPosition = anchoredPosition;

            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.12f, 0.16f, 0.22f, 0.95f);

            Button button = buttonObject.AddComponent<Button>();
            UnityEventTools.AddPersistentListener(button.onClick, action);

            GameObject textObject = new("Text");
            textObject.transform.SetParent(buttonObject.transform, false);

            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text text = textObject.AddComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
