using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZombieOverdrive.Combat;
using ZombieOverdrive.Core;
using ZombieOverdrive.Enemies;
using ZombieOverdrive.Pickups;
using ZombieOverdrive.UI;
using ZombieOverdrive.Utility;

public static class ZombieOverdriveSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/Main.unity";
    private const string PrefabRoot = "Assets/Prefabs";
    private const string ArtRoot = "Assets/Art/Generated";

    [MenuItem("Zombie Overdrive/Build Prototype Scene")]
    public static void BuildPrototypeScene()
    {
        EnsureFolders();
        int playerLayer = EnsureLayer("Player");
        int enemyLayer = EnsureLayer("Enemy");
        int pickupLayer = EnsureLayer("Pickup");
        int projectileLayer = EnsureLayer("Projectile");

        Sprite playerSprite = CreateSprite("player", new Color(0.25f, 0.75f, 1f, 1f), SpriteShape.Circle);
        Sprite walkerSprite = CreateSprite("walker", new Color(0.25f, 0.85f, 0.35f, 1f), SpriteShape.Circle);
        Sprite runnerSprite = CreateSprite("runner", new Color(1f, 0.45f, 0.25f, 1f), SpriteShape.Circle);
        Sprite spitterSprite = CreateSprite("spitter", new Color(0.95f, 0.85f, 0.25f, 1f), SpriteShape.Circle);
        Sprite tankerSprite = CreateSprite("tanker", new Color(0.55f, 0.6f, 0.65f, 1f), SpriteShape.Circle);
        Sprite bossSprite = CreateSprite("boss", new Color(0.9f, 0.15f, 0.25f, 1f), SpriteShape.Circle);
        Sprite bulletSprite = CreateSprite("bullet", new Color(1f, 0.9f, 0.25f, 1f), SpriteShape.Diamond);
        Sprite acidSprite = CreateSprite("acid", new Color(0.65f, 1f, 0.2f, 1f), SpriteShape.Diamond);
        Sprite orbSprite = CreateSprite("singularity_orb", new Color(0.45f, 0.2f, 1f, 1f), SpriteShape.Circle);
        Sprite xpSprite = CreateSprite("xp_crystal", new Color(0.25f, 0.55f, 1f, 1f), SpriteShape.Diamond);
        Sprite tileSprite = CreateSprite("ground_tile", new Color(0.12f, 0.14f, 0.16f, 1f), SpriteShape.Square);

        GameObject bulletPrefab = CreateBulletPrefab(bulletSprite, projectileLayer);
        GameObject acidPrefab = CreateAcidPrefab(acidSprite, projectileLayer);
        GameObject orbPrefab = CreateSingularityOrbPrefab(orbSprite, projectileLayer);
        GameObject walkerPrefab = CreateEnemyPrefab("Walker", walkerSprite, enemyLayer, 0.75f, null);
        GameObject runnerPrefab = CreateEnemyPrefab("Runner", runnerSprite, enemyLayer, 0.58f, null);
        GameObject spitterPrefab = CreateEnemyPrefab("Spitter", spitterSprite, enemyLayer, 0.85f, acidPrefab);
        GameObject tankerPrefab = CreateEnemyPrefab("Tanker", tankerSprite, enemyLayer, 1.25f, null);
        GameObject mutantBossPrefab = CreateEnemyPrefab("MutantBoss", bossSprite, enemyLayer, 2.1f, null);
        GameObject finalBossPrefab = CreateEnemyPrefab("FinalBoss", bossSprite, enemyLayer, 2.7f, acidPrefab);
        GameObject xpPrefab = CreateExperiencePrefab(xpSprite, pickupLayer);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Main";

        InfiniteGround2D ground = CreateGround(tileSprite);

        GameObject player = CreatePlayer(playerSprite, playerLayer, pickupLayer, bulletPrefab, orbPrefab, enemyLayer);
        CameraFollow2D cameraFollow = CreateCamera(player.transform);
        ground.SetTarget(player.transform);
        CreateEventSystem();

        GameObject poolsRoot = new GameObject("Pools");
        GameObjectPool bulletPool = CreatePool("Bullet Pool", bulletPrefab, 80, poolsRoot.transform);
        GameObjectPool walkerPool = CreatePool("Walker Pool", walkerPrefab, 80, poolsRoot.transform);
        GameObjectPool runnerPool = CreatePool("Runner Pool", runnerPrefab, 50, poolsRoot.transform);
        GameObjectPool spitterPool = CreatePool("Spitter Pool", spitterPrefab, 35, poolsRoot.transform);
        GameObjectPool tankerPool = CreatePool("Tanker Pool", tankerPrefab, 16, poolsRoot.transform);
        GameObjectPool mutantBossPool = CreatePool("Mutant Boss Pool", mutantBossPrefab, 1, poolsRoot.transform);
        GameObjectPool finalBossPool = CreatePool("Final Boss Pool", finalBossPrefab, 1, poolsRoot.transform);
        GameObjectPool xpPool = CreatePool("Experience Pool", xpPrefab, 220, poolsRoot.transform);

        PistolWeapon pistol = player.GetComponent<PistolWeapon>();
        SetObjectField(pistol, "bulletPool", bulletPool);
        ShotgunWeapon shotgun = player.GetComponent<ShotgunWeapon>();
        SetObjectField(shotgun, "bulletPool", bulletPool);

        GameObject managers = new GameObject("Game Systems");
        LevelSystem levelSystem = player.GetComponent<LevelSystem>();
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        PlayerCollector playerCollector = player.GetComponent<PlayerCollector>();
        UpgradeSystem upgradeSystem = managers.AddComponent<UpgradeSystem>();
        WaveSpawner waveSpawner = managers.AddComponent<WaveSpawner>();
        GameManager gameManager = managers.AddComponent<GameManager>();

        SetObjectField(waveSpawner, "walkerPool", walkerPool);
        SetObjectField(waveSpawner, "runnerPool", runnerPool);
        SetObjectField(waveSpawner, "spitterPool", spitterPool);
        SetObjectField(waveSpawner, "tankerPool", tankerPool);
        SetObjectField(waveSpawner, "mutantBossPool", mutantBossPool);
        SetObjectField(waveSpawner, "finalBossPool", finalBossPool);
        SetObjectField(waveSpawner, "experiencePool", xpPool);

        Canvas canvas = CreateCanvas();
        GameHud hud = CreateHud(canvas.transform);
        UpgradePanel upgradePanel = CreateUpgradePanel(canvas.transform);
        PauseMenu pauseMenu = CreatePauseMenu(canvas.transform);

        SetObjectField(gameManager, "playerMovement", playerMovement);
        SetObjectField(gameManager, "playerHealth", playerHealth);
        SetObjectField(gameManager, "playerCollector", playerCollector);
        SetObjectField(gameManager, "levelSystem", levelSystem);
        SetObjectField(gameManager, "upgradeSystem", upgradeSystem);
        SetObjectField(gameManager, "pistolWeapon", pistol);
        SetArrayField(gameManager, "weapons", player.GetComponents<WeaponBase>());
        SetObjectField(gameManager, "waveSpawner", waveSpawner);
        SetObjectField(gameManager, "hud", hud);
        SetObjectField(gameManager, "upgradePanel", upgradePanel);
        SetObjectField(gameManager, "pauseMenu", pauseMenu);

        cameraFollow.SetTarget(player.transform);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Zombie Overdrive prototype scene generated at " + ScenePath);
    }

    [MenuItem("Zombie Overdrive/Smoke Test Prototype")]
    public static void SmokeTestPrototype()
    {
        BuildPrototypeScene();
        EditorApplication.isPlaying = true;
        double startTime = EditorApplication.timeSinceStartup;
        EditorApplication.update += StopAfterDelay;

        void StopAfterDelay()
        {
            if (EditorApplication.timeSinceStartup - startTime < 5d)
            {
                return;
            }

            EditorApplication.update -= StopAfterDelay;
            EditorApplication.isPlaying = false;
            Debug.Log("Zombie Overdrive smoke test completed.");
        }
    }

    [MenuItem("Zombie Overdrive/Validate Prototype Scene")]
    public static void ValidatePrototypeScene()
    {
        if (!File.Exists(ScenePath))
        {
            throw new FileNotFoundException("Missing prototype scene", ScenePath);
        }

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            throw new System.InvalidOperationException("Could not open prototype scene.");
        }

        RequireObject<GameManager>("GameManager");
        RequireObject<PlayerMovement>("PlayerMovement");
        RequireObject<PlayerHealth>("PlayerHealth");
        RequireObject<PistolWeapon>("PistolWeapon");
        RequireObject<ShotgunWeapon>("ShotgunWeapon");
        RequireObject<TeslaWeapon>("TeslaWeapon");
        RequireObject<SingularityWeapon>("SingularityWeapon");
        RequireObject<LightbladeWeapon>("LightbladeWeapon");
        RequireObject<LaserWeapon>("LaserWeapon");
        RequireObject<WaveSpawner>("WaveSpawner");
        RequireObject<GameHud>("GameHud");
        RequireObject<UpgradePanel>("UpgradePanel");
        RequireObject<PauseMenu>("PauseMenu");
        RequireObject<InfiniteGround2D>("InfiniteGround2D");

        RequireAsset<GameObject>("Assets/Prefabs/Bullet.prefab");
        RequireAsset<GameObject>("Assets/Prefabs/AcidProjectile.prefab");
        RequireAsset<GameObject>("Assets/Prefabs/SingularityOrb.prefab");
        RequireAsset<GameObject>("Assets/Prefabs/Walker.prefab");
        RequireAsset<GameObject>("Assets/Prefabs/Runner.prefab");
        RequireAsset<GameObject>("Assets/Prefabs/Spitter.prefab");
        RequireAsset<GameObject>("Assets/Prefabs/Tanker.prefab");
        RequireAsset<GameObject>("Assets/Prefabs/MutantBoss.prefab");
        RequireAsset<GameObject>("Assets/Prefabs/FinalBoss.prefab");
        RequireAsset<GameObject>("Assets/Prefabs/ExperienceCrystal.prefab");

        bool sceneInBuild = false;
        foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
        {
            if (buildScene.path == ScenePath && buildScene.enabled)
            {
                sceneInBuild = true;
                break;
            }
        }

        if (!sceneInBuild)
        {
            throw new System.InvalidOperationException("Main scene is not enabled in Build Settings.");
        }

        Debug.Log("Zombie Overdrive prototype validation passed.");
    }

    private static void RequireObject<T>(string label) where T : Object
    {
        T[] objects = Object.FindObjectsOfType<T>(true);
        if (objects == null || objects.Length == 0)
        {
            throw new System.InvalidOperationException("Missing scene object: " + label);
        }
    }

    private static void RequireAsset<T>(string path) where T : Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            throw new FileNotFoundException("Missing asset", path);
        }
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets", "Art");
        EnsureFolder("Assets/Art", "Generated");
        EnsureFolder("Assets", "Prefabs");
        EnsureFolder("Assets", "Scenes");
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    private static int EnsureLayer(string layerName)
    {
        int existing = LayerMask.NameToLayer(layerName);
        if (existing >= 0)
        {
            return existing;
        }

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(layer.stringValue))
            {
                layer.stringValue = layerName;
                tagManager.ApplyModifiedPropertiesWithoutUndo();
                return i;
            }
        }

        Debug.LogWarning("No free layer slot for " + layerName);
        return 0;
    }

    private enum SpriteShape
    {
        Circle,
        Diamond,
        Square
    }

    private static Sprite CreateSprite(string name, Color color, SpriteShape shape)
    {
        string path = $"{ArtRoot}/{name}.png";
        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color clear = new Color(0f, 0f, 0f, 0f);
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x, y);
                bool fill = shape == SpriteShape.Square;
                if (shape == SpriteShape.Circle)
                {
                    fill = Vector2.Distance(p, center) <= 28f;
                }
                else if (shape == SpriteShape.Diamond)
                {
                    fill = Mathf.Abs(x - center.x) + Mathf.Abs(y - center.y) <= 28f;
                }

                texture.SetPixel(x, y, fill ? color : clear);
            }
        }

        texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);

        AssetDatabase.ImportAsset(path);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 32f;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static GameObject CreateBulletPrefab(Sprite sprite, int layer)
    {
        GameObject bullet = new GameObject("Bullet");
        bullet.layer = layer;
        SpriteRenderer renderer = bullet.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 8;
        bullet.transform.localScale = new Vector3(0.35f, 0.18f, 1f);
        BoxCollider2D collider = bullet.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        bullet.AddComponent<Bullet>();
        bullet.AddComponent<Poolable>();
        return SavePrefab(bullet, "Bullet.prefab");
    }

    private static GameObject CreateAcidPrefab(Sprite sprite, int layer)
    {
        GameObject acid = new GameObject("Acid Projectile");
        acid.layer = layer;
        SpriteRenderer renderer = acid.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 7;
        acid.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
        CircleCollider2D collider = acid.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.35f;
        acid.AddComponent<AcidProjectile>();
        return SavePrefab(acid, "AcidProjectile.prefab");
    }

    private static GameObject CreateSingularityOrbPrefab(Sprite sprite, int layer)
    {
        GameObject orb = new GameObject("Singularity Orb");
        orb.layer = layer;
        SpriteRenderer renderer = orb.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 6;
        orb.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
        CircleCollider2D collider = orb.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.55f;
        orb.AddComponent<SingularityOrb>();
        return SavePrefab(orb, "SingularityOrb.prefab");
    }

    private static GameObject CreateEnemyPrefab(string name, Sprite sprite, int layer, float scale, GameObject acidProjectilePrefab)
    {
        GameObject enemy = new GameObject(name);
        enemy.layer = layer;
        enemy.transform.localScale = Vector3.one * scale;
        SpriteRenderer renderer = enemy.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 5;
        Rigidbody2D body = enemy.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        CircleCollider2D collider = enemy.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.45f;
        EnemyController controller = enemy.AddComponent<EnemyController>();
        enemy.AddComponent<EnemyHealth>();
        enemy.AddComponent<Poolable>();
        SetLayerMask(controller, "enemyMask", 1 << layer);
        if (acidProjectilePrefab != null)
        {
            SetObjectField(controller, "acidProjectilePrefab", acidProjectilePrefab);
        }

        return SavePrefab(enemy, name + ".prefab");
    }

    private static GameObject CreateExperiencePrefab(Sprite sprite, int layer)
    {
        GameObject pickup = new GameObject("Experience Crystal");
        pickup.layer = layer;
        pickup.transform.localScale = Vector3.one * 0.35f;
        SpriteRenderer renderer = pickup.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 4;
        CircleCollider2D collider = pickup.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.6f;
        pickup.AddComponent<ExperiencePickup>();
        pickup.AddComponent<Poolable>();
        return SavePrefab(pickup, "ExperienceCrystal.prefab");
    }

    private static GameObject SavePrefab(GameObject instance, string fileName)
    {
        string path = $"{PrefabRoot}/{fileName}";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
        return prefab;
    }

    private static GameObject CreatePlayer(Sprite sprite, int playerLayer, int pickupLayer, GameObject bulletPrefab, GameObject orbPrefab, int enemyLayer)
    {
        GameObject player = new GameObject("Player");
        player.layer = playerLayer;
        SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 10;

        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        CircleCollider2D collider = player.AddComponent<CircleCollider2D>();
        collider.radius = 0.45f;
        collider.isTrigger = true;

        player.AddComponent<PlayerMovement>();
        player.AddComponent<PlayerHealth>();
        PlayerCollector collector = player.AddComponent<PlayerCollector>();
        SetLayerMask(collector, "pickupMask", 1 << pickupLayer);
        player.AddComponent<LevelSystem>();

        GameObject muzzle = new GameObject("Muzzle");
        muzzle.transform.SetParent(player.transform);
        muzzle.transform.localPosition = new Vector3(0.55f, 0f, 0f);

        PistolWeapon pistol = player.AddComponent<PistolWeapon>();
        SetObjectField(pistol, "muzzle", muzzle.transform);
        ShotgunWeapon shotgun = player.AddComponent<ShotgunWeapon>();
        SetObjectField(shotgun, "muzzle", muzzle.transform);
        TeslaWeapon tesla = player.AddComponent<TeslaWeapon>();
        SetLayerMask(tesla, "enemyMask", 1 << enemyLayer);
        SingularityWeapon singularity = player.AddComponent<SingularityWeapon>();
        SetObjectField(singularity, "orbPrefab", orbPrefab);
        SetLayerMask(singularity, "enemyMask", 1 << enemyLayer);
        LightbladeWeapon lightblade = player.AddComponent<LightbladeWeapon>();
        SetLayerMask(lightblade, "enemyMask", 1 << enemyLayer);
        LaserWeapon laser = player.AddComponent<LaserWeapon>();
        SetLayerMask(laser, "enemyMask", 1 << enemyLayer);

        return player;
    }

    private static InfiniteGround2D CreateGround(Sprite sprite)
    {
        GameObject root = new GameObject("Infinite Ground Placeholder");
        InfiniteGround2D infiniteGround = root.AddComponent<InfiniteGround2D>();
        const float tileSize = 36f;
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                GameObject tile = new GameObject($"Ground Tile {x},{y}");
                tile.transform.SetParent(root.transform);
                tile.transform.position = new Vector3(x * tileSize, y * tileSize, 1f);
                tile.transform.localScale = Vector3.one * (tileSize / 2f);
                SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = -10;
                renderer.color = ((x + y) & 1) == 0 ? Color.white : new Color(0.85f, 0.9f, 1f, 1f);
            }
        }

        return infiniteGround;
    }

    private static CameraFollow2D CreateCamera(Transform player)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 6f;
        camera.backgroundColor = new Color(0.04f, 0.045f, 0.055f, 1f);
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(player.position.x, player.position.y, -10f);
        AudioListener listener = cameraObject.AddComponent<AudioListener>();
        CameraFollow2D follow = cameraObject.AddComponent<CameraFollow2D>();
        follow.SetTarget(player);
        return follow;
    }

    private static GameObjectPool CreatePool(string name, GameObject prefab, int prewarm, Transform parent)
    {
        GameObject poolObject = new GameObject(name);
        poolObject.transform.SetParent(parent);
        GameObjectPool pool = poolObject.AddComponent<GameObjectPool>();
        SetObjectField(pool, "prefab", prefab);
        SetIntField(pool, "prewarmCount", prewarm);
        return pool;
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static GameHud CreateHud(Transform parent)
    {
        GameObject hudObject = new GameObject("HUD");
        hudObject.transform.SetParent(parent, false);
        RectTransform hudRect = hudObject.AddComponent<RectTransform>();
        hudRect.anchorMin = Vector2.zero;
        hudRect.anchorMax = Vector2.one;
        hudRect.offsetMin = Vector2.zero;
        hudRect.offsetMax = Vector2.zero;
        GameHud hud = hudObject.AddComponent<GameHud>();

        Text timer = CreateText(hudObject.transform, "Timer", "10:00", 42, TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(220f, 60f));
        Text health = CreateText(hudObject.transform, "Health Text", "HP", 24, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(24f, -22f), new Vector2(260f, 40f));
        Text level = CreateText(hudObject.transform, "Level Text", "Lv 1", 24, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(24f, -92f), new Vector2(120f, 36f));
        Text kills = CreateText(hudObject.transform, "Kills Text", "Kills 0", 24, TextAnchor.UpperRight, new Vector2(1f, 1f), new Vector2(-24f, -22f), new Vector2(220f, 40f));
        Text message = CreateText(hudObject.transform, "Message", "", 48, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 120f));

        Slider hpSlider = CreateSlider(hudObject.transform, "Health Bar", new Vector2(0f, 1f), new Vector2(24f, -62f), new Vector2(280f, 22f), new Color(0.85f, 0.1f, 0.12f, 1f));
        Slider xpSlider = CreateSlider(hudObject.transform, "XP Bar", new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(800f, 24f), new Color(0.2f, 0.55f, 1f, 1f));

        SetObjectField(hud, "timerText", timer);
        SetObjectField(hud, "healthText", health);
        SetObjectField(hud, "levelText", level);
        SetObjectField(hud, "killText", kills);
        SetObjectField(hud, "messageText", message);
        SetObjectField(hud, "healthSlider", hpSlider);
        SetObjectField(hud, "xpSlider", xpSlider);
        return hud;
    }

    private static UpgradePanel CreateUpgradePanel(Transform parent)
    {
        GameObject panel = new GameObject("Upgrade Panel");
        panel.transform.SetParent(parent, false);
        Image background = panel.AddComponent<Image>();
        background.color = new Color(0.02f, 0.025f, 0.035f, 0.94f);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(980f, 520f);

        CreateText(panel.transform, "Title", "Choose an Upgrade", 42, TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(600f, 70f));

        UpgradePanel upgradePanel = panel.AddComponent<UpgradePanel>();
        Button[] buttons = new Button[3];
        Text[] titles = new Text[3];
        Text[] descriptions = new Text[3];

        for (int i = 0; i < 3; i++)
        {
            GameObject buttonObject = new GameObject("Option " + (i + 1));
            buttonObject.transform.SetParent(panel.transform, false);
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.12f, 0.14f, 0.18f, 1f);
            Button button = buttonObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.2f, 0.25f, 0.35f, 1f);
            colors.pressedColor = new Color(0.08f, 0.1f, 0.14f, 1f);
            button.colors = colors;

            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = new Vector2((i - 1) * 300f, -40f);
            buttonRect.sizeDelta = new Vector2(260f, 300f);

            titles[i] = CreateText(buttonObject.transform, "Title", "Upgrade", 26, TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(220f, 70f));
            descriptions[i] = CreateText(buttonObject.transform, "Description", "Description", 21, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.45f), new Vector2(0f, 0f), new Vector2(220f, 170f));
            buttons[i] = button;
        }

        SetArrayField(upgradePanel, "optionButtons", buttons);
        SetArrayField(upgradePanel, "titleTexts", titles);
        SetArrayField(upgradePanel, "descriptionTexts", descriptions);
        panel.SetActive(false);
        return upgradePanel;
    }

    private static PauseMenu CreatePauseMenu(Transform parent)
    {
        GameObject panel = new GameObject("Pause Menu");
        panel.transform.SetParent(parent, false);
        Image background = panel.AddComponent<Image>();
        background.color = new Color(0.02f, 0.025f, 0.035f, 0.94f);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(760f, 620f);

        CreateText(panel.transform, "Title", "Paused", 44, TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(500f, 70f));
        Text status = CreateText(panel.transform, "Status", "", 22, TextAnchor.UpperLeft, new Vector2(0.5f, 1f), new Vector2(-330f, -100f), new Vector2(660f, 330f));
        Button resume = CreateMenuButton(panel.transform, "Resume Button", "Resume", new Vector2(-220f, -245f));
        Button restart = CreateMenuButton(panel.transform, "Restart Button", "Restart", new Vector2(0f, -245f));
        Button quit = CreateMenuButton(panel.transform, "Quit Button", "Quit", new Vector2(220f, -245f));

        PauseMenu pauseMenu = panel.AddComponent<PauseMenu>();
        SetObjectField(pauseMenu, "statusText", status);
        SetObjectField(pauseMenu, "resumeButton", resume);
        SetObjectField(pauseMenu, "restartButton", restart);
        SetObjectField(pauseMenu, "quitButton", quit);
        panel.SetActive(false);
        return pauseMenu;
    }

    private static Button CreateMenuButton(Transform parent, string name, string label, Vector2 position)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.12f, 0.14f, 0.18f, 1f);
        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.2f, 0.25f, 0.35f, 1f);
        colors.pressedColor = new Color(0.08f, 0.1f, 0.14f, 1f);
        button.colors = colors;

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(180f, 70f);
        CreateText(buttonObject.transform, "Label", label, 26, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(160f, 50f));
        return button;
    }

    private static Text CreateText(Transform parent, string name, string text, int fontSize, TextAnchor alignment, Vector2 anchor, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        Text uiText = textObject.AddComponent<Text>();
        uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        uiText.text = text;
        uiText.fontSize = fontSize;
        uiText.color = Color.white;
        uiText.alignment = alignment;
        uiText.horizontalOverflow = HorizontalWrapMode.Wrap;
        uiText.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return uiText;
    }

    private static Slider CreateSlider(Transform parent, string name, Vector2 anchor, Vector2 anchoredPosition, Vector2 size, Color fillColor)
    {
        GameObject sliderObject = new GameObject(name);
        sliderObject.transform.SetParent(parent, false);
        RectTransform rect = sliderObject.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Slider slider = sliderObject.AddComponent<Slider>();
        slider.transition = Selectable.Transition.None;

        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderObject.transform, false);
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = new Color(0.06f, 0.065f, 0.08f, 0.95f);
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(2f, 2f);
        fillAreaRect.offsetMax = new Vector2(-2f, -2f);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = fillColor;
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        return slider;
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    private static void SetObjectField(Object target, string fieldName, Object value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(fieldName);
        property.objectReferenceValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetIntField(Object target, string fieldName, int value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(fieldName);
        property.intValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetLayerMask(Object target, string fieldName, int mask)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(fieldName);
        property.intValue = mask;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetArrayField<T>(Object target, string fieldName, T[] values) where T : Object
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(fieldName);
        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }
}
