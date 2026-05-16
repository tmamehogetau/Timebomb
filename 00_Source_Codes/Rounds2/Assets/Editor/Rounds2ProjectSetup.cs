using FishNet.Component.Spawning;
using FishNet.Component.Transforming;
using FishNet.Managing;
using FishNet.Managing.Predicting;
using FishNet.Managing.Timing;
using FishNet.Object;
using Rounds2.Combat;
using Rounds2.Config;
using Rounds2.Development;
using Rounds2.Match;
using Rounds2.Networking;
using Rounds2.Player;
using Rounds2.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
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

        public static void CreatePlayerPrefab()
        {
            GameObject playerObject = new("Player");
            NetworkObject networkObject = playerObject.AddComponent<NetworkObject>();
            ConfigureLowLatencyNetworkObject(networkObject);
            ConfigureRigidbody2DPrediction(networkObject);

            Rigidbody2D body = playerObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.None;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CircleCollider2D collider = playerObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.45f;

            SpriteRenderer renderer = playerObject.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateUnitSprite();
            renderer.color = new Color(0.2f, 0.8f, 1f, 1f);

            playerObject.AddComponent<HealthVisuals>();
            playerObject.AddComponent<Health>();
            playerObject.AddComponent<PredictedPlayerMotor>();
            playerObject.AddComponent<PlayerInputReader>();
            playerObject.AddComponent<PlayerController>();
            playerObject.AddComponent<PlayerBotController>();
            playerObject.AddComponent<PlayerDevelopmentShortcuts>();
            playerObject.AddComponent<PlayerOwnerVisuals>();
            playerObject.AddComponent<WeaponController>();

            CreateAimIndicator(playerObject.transform);

            GameObject muzzleObject = new("Muzzle");
            muzzleObject.transform.SetParent(playerObject.transform, false);
            muzzleObject.transform.localPosition = new Vector3(CombatTuning.MuzzleForwardOffset, 0f, 0f);

            PrefabUtility.SaveAsPrefabAsset(playerObject, "Assets/Prefabs/Player.prefab");
            Object.DestroyImmediate(playerObject);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void CreateBulletPrefabAndWirePlayer()
        {
            Bullet bulletPrefab = CreateBulletPrefab();
            WirePlayerWeapon(bulletPrefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void CreateArenaScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Arena01";

            SetManager setManager = CreateSetManager();
            Transform[] spawnPoints = CreateSpawnPoints();
            CreatePlayerSpawnManager(networkManager: null, setManager, spawnPoints);
            CreateArenaCamera();
            CreateArenaBounds();
            CreateDebugHud(networkManager: null);

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Arena01.unity");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void ConfigureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Bootstrap.unity", enabled: true),
                new EditorBuildSettingsScene("Assets/Scenes/Arena01.unity", enabled: true)
            };

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
                RemoveDefaultPlayerSpawner(instance);
                NetworkManager networkManager = instance.GetComponent<NetworkManager>();
                ConfigureNetworkManagerTiming(networkManager);
                return networkManager;
            }

            GameObject fallback = new("NetworkManager");
            NetworkManager fallbackNetworkManager = fallback.AddComponent<NetworkManager>();
            ConfigureNetworkManagerTiming(fallbackNetworkManager);
            return fallbackNetworkManager;
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
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
        }

        private static void RemoveDefaultPlayerSpawner(GameObject networkManagerObject)
        {
            PlayerSpawner[] spawners = networkManagerObject.GetComponentsInChildren<PlayerSpawner>();
            foreach (PlayerSpawner spawner in spawners)
            {
                Object.DestroyImmediate(spawner);
            }
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

        private static SetManager CreateSetManager()
        {
            GameObject setManagerObject = new("SetManager");
            setManagerObject.AddComponent<NetworkObject>();
            return setManagerObject.AddComponent<SetManager>();
        }

        private static Transform[] CreateSpawnPoints()
        {
            Transform spawnA = CreateSpawnPoint("SpawnA", new Vector3(-4f, 0f, 0f));
            Transform spawnB = CreateSpawnPoint("SpawnB", new Vector3(4f, 0f, 0f));
            return new[] { spawnA, spawnB };
        }

        private static Transform CreateSpawnPoint(string name, Vector3 position)
        {
            GameObject spawnObject = new(name);
            spawnObject.transform.position = position;
            return spawnObject.transform;
        }

        private static void CreatePlayerSpawnManager(NetworkManager networkManager, SetManager setManager, Transform[] spawnPoints)
        {
            GameObject managerObject = new("PlayerSpawnManager");
            PlayerSpawnManager spawnManager = managerObject.AddComponent<PlayerSpawnManager>();
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");

            SerializedObject serializedSpawnManager = new(spawnManager);
            serializedSpawnManager.FindProperty("networkManager").objectReferenceValue = networkManager;
            serializedSpawnManager.FindProperty("playerPrefab").objectReferenceValue = playerPrefab.GetComponent<NetworkObject>();
            serializedSpawnManager.FindProperty("setManager").objectReferenceValue = setManager;

            SerializedProperty serializedSpawnPoints = serializedSpawnManager.FindProperty("spawnPoints");
            serializedSpawnPoints.arraySize = spawnPoints.Length;
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                serializedSpawnPoints.GetArrayElementAtIndex(i).objectReferenceValue = spawnPoints[i];
            }

            serializedSpawnManager.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateArenaCamera()
        {
            GameObject cameraObject = new("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.05f, 0.06f, 0.07f, 1f);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        }

        private static void CreateArenaBounds()
        {
            Sprite wallSprite = CreateSquareSprite("Assets/Prefabs/ArenaWallSprite.png");
            CreateWall("TopWall", new Vector3(0f, 5.1f, 0f), new Vector3(10.5f, 0.3f, 1f), wallSprite);
            CreateWall("BottomWall", new Vector3(0f, -5.1f, 0f), new Vector3(10.5f, 0.3f, 1f), wallSprite);
            CreateWall("LeftWall", new Vector3(-5.1f, 0f, 0f), new Vector3(0.3f, 10.5f, 1f), wallSprite);
            CreateWall("RightWall", new Vector3(5.1f, 0f, 0f), new Vector3(0.3f, 10.5f, 1f), wallSprite);
        }

        private static void CreateWall(string name, Vector3 position, Vector3 scale, Sprite sprite)
        {
            GameObject wallObject = new(name);
            wallObject.transform.position = position;
            wallObject.transform.localScale = scale;

            SpriteRenderer renderer = wallObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(0.35f, 0.38f, 0.42f, 1f);

            wallObject.AddComponent<BoxCollider2D>();
        }

        private static void CreateDebugHud(NetworkManager networkManager)
        {
            GameObject canvasObject = new("DebugHudCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject textObject = new("StatusText");
            textObject.transform.SetParent(canvasObject.transform, false);

            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(1f, 1f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.pivot = new Vector2(1f, 1f);
            textRect.anchoredPosition = new Vector2(-16f, -16f);
            textRect.sizeDelta = new Vector2(220f, 64f);

            Text statusText = textObject.AddComponent<Text>();
            statusText.text = DebugHudText.Format(serverStarted: false, clientStarted: false);
            statusText.alignment = TextAnchor.UpperRight;
            statusText.color = Color.white;
            statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusText.fontSize = 18;

            DebugHud debugHud = canvasObject.AddComponent<DebugHud>();
            SerializedObject serializedHud = new(debugHud);
            serializedHud.FindProperty("networkManager").objectReferenceValue = networkManager;
            serializedHud.FindProperty("statusText").objectReferenceValue = statusText;
            serializedHud.ApplyModifiedPropertiesWithoutUndo();

            Text scoreText = CreateScoreText(canvasObject.transform);
            ScoreHud scoreHud = canvasObject.AddComponent<ScoreHud>();
            SerializedObject serializedScoreHud = new(scoreHud);
            serializedScoreHud.FindProperty("scoreText").objectReferenceValue = scoreText;
            serializedScoreHud.ApplyModifiedPropertiesWithoutUndo();

            Text healthText = CreateHealthText(canvasObject.transform);
            HealthHud healthHud = canvasObject.AddComponent<HealthHud>();
            SerializedObject serializedHealthHud = new(healthHud);
            serializedHealthHud.FindProperty("healthText").objectReferenceValue = healthText;
            serializedHealthHud.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateAimIndicator(Transform parent)
        {
            GameObject indicatorObject = new("AimIndicator");
            indicatorObject.transform.SetParent(parent, false);
            indicatorObject.transform.localPosition = new Vector3(
                CombatTuning.MuzzleForwardOffset + CombatTuning.AimIndicatorLength * 0.5f,
                0f,
                0f);
            indicatorObject.transform.localScale = new Vector3(
                CombatTuning.AimIndicatorLength,
                CombatTuning.AimIndicatorThickness,
                1f);

            SpriteRenderer renderer = indicatorObject.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateSquareSprite("Assets/Prefabs/AimIndicatorSprite.png");
            renderer.color = new Color(1f, 1f, 1f, 0.78f);
            renderer.sortingOrder = 2;
        }

        private static Text CreateScoreText(Transform parent)
        {
            GameObject textObject = new("ScoreText");
            textObject.transform.SetParent(parent, false);

            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 1f);
            textRect.anchorMax = new Vector2(0.5f, 1f);
            textRect.pivot = new Vector2(0.5f, 1f);
            textRect.anchoredPosition = new Vector2(0f, -16f);
            textRect.sizeDelta = new Vector2(320f, 64f);

            Text scoreText = textObject.AddComponent<Text>();
            scoreText.text = ScoreHudText.Format(0, 0, MatchTuning.RoundWinsToWinMatch, matchFinished: false, winnerLabel: "");
            scoreText.alignment = TextAnchor.UpperCenter;
            scoreText.color = Color.white;
            scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            scoreText.fontSize = 22;
            return scoreText;
        }

        private static Text CreateHealthText(Transform parent)
        {
            GameObject textObject = new("HealthText");
            textObject.transform.SetParent(parent, false);

            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0f);
            textRect.anchorMax = new Vector2(0.5f, 0f);
            textRect.pivot = new Vector2(0.5f, 0f);
            textRect.anchoredPosition = new Vector2(0f, 18f);
            textRect.sizeDelta = new Vector2(760f, 40f);

            Text healthText = textObject.AddComponent<Text>();
            healthText.text = HealthHudText.Format(
                CombatTuning.BaseHealth,
                CombatTuning.BaseHealth,
                CombatTuning.BaseHealth,
                CombatTuning.MagazineSize,
                CombatTuning.MagazineSize,
                CombatTuning.MagazineSize,
                leftReloading: false,
                rightReloading: false);
            healthText.alignment = TextAnchor.LowerCenter;
            healthText.color = Color.white;
            healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            healthText.fontSize = 20;
            return healthText;
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

        private static Sprite CreateUnitSprite()
        {
            const string assetPath = "Assets/Prefabs/PlayerUnitSprite.png";
            Texture2D texture = new(32, 32, TextureFormat.RGBA32, false);
            Color clear = new(0f, 0f, 0f, 0f);
            Color fill = Color.white;
            Vector2 center = new(15.5f, 15.5f);

            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    texture.SetPixel(x, y, distance <= 14f ? fill : clear);
                }
            }

            texture.Apply();
            byte[] png = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(assetPath, png);
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(assetPath);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32f;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static Bullet CreateBulletPrefab()
        {
            GameObject bulletObject = new("Bullet");
            ConfigureLowLatencyNetworkObject(bulletObject.AddComponent<NetworkObject>());
            ConfigureServerAuthoritativeTransform(bulletObject.AddComponent<NetworkTransform>(), sendToOwner: true);

            Rigidbody2D body = bulletObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CircleCollider2D collider = bulletObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.12f;
            collider.isTrigger = true;

            SpriteRenderer renderer = bulletObject.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateCircleSprite("Assets/Prefabs/BulletSprite.png", 16, 7f);
            renderer.color = new Color(1f, 0.92f, 0.25f, 1f);

            Bullet bullet = bulletObject.AddComponent<Bullet>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(bulletObject, "Assets/Prefabs/Bullet.prefab");
            Object.DestroyImmediate(bulletObject);
            return prefab.GetComponent<Bullet>();
        }

        private static void WirePlayerWeapon(Bullet bulletPrefab)
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
            GameObject playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);

            WeaponController weapon = playerInstance.GetComponent<WeaponController>();
            if (weapon == null)
            {
                weapon = playerInstance.AddComponent<WeaponController>();
            }
            else
            {
                WeaponController[] weapons = playerInstance.GetComponents<WeaponController>();
                for (int i = 1; i < weapons.Length; i++)
                {
                    Object.DestroyImmediate(weapons[i]);
                }
            }

            Transform muzzle = playerInstance.transform.Find("Muzzle");
            if (muzzle == null)
            {
                GameObject muzzleObject = new("Muzzle");
                muzzleObject.transform.SetParent(playerInstance.transform, false);
                muzzleObject.transform.localPosition = new Vector3(CombatTuning.MuzzleForwardOffset, 0f, 0f);
                muzzle = muzzleObject.transform;
            }

            SerializedObject serializedWeapon = new(weapon);
            serializedWeapon.FindProperty("bulletPrefab").objectReferenceValue = bulletPrefab;
            serializedWeapon.FindProperty("muzzle").objectReferenceValue = muzzle;
            serializedWeapon.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(playerInstance, "Assets/Prefabs/Player.prefab");
            Object.DestroyImmediate(playerInstance);
        }

        private static void ConfigureServerAuthoritativeTransform(NetworkTransform networkTransform, bool sendToOwner)
        {
            SerializedObject serializedTransform = new(networkTransform);
            serializedTransform.FindProperty("_clientAuthoritative").boolValue = false;
            serializedTransform.FindProperty("_sendToOwner").boolValue = sendToOwner;
            serializedTransform.FindProperty("_interval").intValue = NetworkTuning.TransformSendIntervalTicks;
            serializedTransform.FindProperty("_interpolation").intValue = NetworkTuning.TransformInterpolationTicks;
            serializedTransform.FindProperty("_extrapolation").intValue = NetworkTuning.TransformExtrapolationTicks;
            serializedTransform.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureLowLatencyNetworkObject(NetworkObject networkObject)
        {
            SerializedObject serializedObject = new(networkObject);
            serializedObject.FindProperty("_adaptiveInterpolation").enumValueIndex = (int)AdaptiveInterpolationType.Off;
            serializedObject.FindProperty("_ownerInterpolation").intValue = NetworkTuning.OwnerInterpolationTicks;
            serializedObject.FindProperty("_spectatorInterpolation").intValue = NetworkTuning.SpectatorInterpolationTicks;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureRigidbody2DPrediction(NetworkObject networkObject)
        {
            SerializedObject serializedObject = new(networkObject);
            serializedObject.FindProperty("_enablePrediction").boolValue = true;
            serializedObject.FindProperty("_predictionType").enumValueIndex = 2;
            serializedObject.FindProperty("_enableStateForwarding").boolValue = true;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureNetworkManagerTiming(NetworkManager networkManager)
        {
            TimeManager timeManager = networkManager.GetComponent<TimeManager>();
            if (timeManager == null)
            {
                timeManager = networkManager.gameObject.AddComponent<TimeManager>();
            }

            SerializedObject serializedTimeManager = new(timeManager);
            serializedTimeManager.FindProperty("_tickRate").intValue = NetworkTuning.TickRate;
            serializedTimeManager.FindProperty("_physicsMode").enumValueIndex = (int)PhysicsMode.TimeManager;
            serializedTimeManager.ApplyModifiedPropertiesWithoutUndo();

            if (networkManager.GetComponent<PredictionManager>() == null)
            {
                networkManager.gameObject.AddComponent<PredictionManager>();
            }
        }

        private static Sprite CreateCircleSprite(string assetPath, int size, float radius)
        {
            Texture2D texture = new(size, size, TextureFormat.RGBA32, false);
            Color clear = new(0f, 0f, 0f, 0f);
            Color fill = Color.white;
            Vector2 center = new((size - 1f) * 0.5f, (size - 1f) * 0.5f);

            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    texture.SetPixel(x, y, distance <= radius ? fill : clear);
                }
            }

            texture.Apply();
            byte[] png = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(assetPath, png);
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(assetPath);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = size;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static Sprite CreateSquareSprite(string assetPath)
        {
            Texture2D texture = new(8, 8, TextureFormat.RGBA32, false);
            Color fill = Color.white;

            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    texture.SetPixel(x, y, fill);
                }
            }

            texture.Apply();
            byte[] png = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(assetPath, png);
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(assetPath);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 8f;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }
    }
}
