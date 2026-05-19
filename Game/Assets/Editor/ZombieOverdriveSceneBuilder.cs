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
    private static readonly string[] IconIds =
    {
        "weapon_pistol",
        "weapon_shotgun",
        "weapon_tesla",
        "weapon_singularity",
        "weapon_lightblade",
        "weapon_laser",
        "evolution_pistol",
        "evolution_shotgun",
        "evolution_tesla",
        "evolution_singularity",
        "evolution_lightblade",
        "evolution_laser",
        "passive_ammobox",
        "passive_overclock",
        "passive_adrenaline",
        "passive_nanoarmor",
        "passive_propellent",
        "passive_gravitycore",
        "passive_magnet",
        "passive_hazmatsuit",
        "passive_greedchip",
        "passive_radar",
        "passive_defibrillator",
        "passive_radio",
        "passive_repair"
    };

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
        Sprite finalBossSprite = CreateSprite("final_boss", new Color(0.55f, 0.1f, 0.8f, 1f), SpriteShape.Circle);
        Sprite bulletSprite = CreateSprite("bullet", new Color(1f, 0.9f, 0.25f, 1f), SpriteShape.Diamond);
        Sprite acidSprite = CreateSprite("acid", new Color(0.65f, 1f, 0.2f, 1f), SpriteShape.Diamond);
        Sprite orbSprite = CreateSprite("singularity_orb", new Color(0.45f, 0.2f, 1f, 1f), SpriteShape.Circle);
        Sprite xpSprite = CreateSprite("xp_crystal", new Color(0.25f, 0.55f, 1f, 1f), SpriteShape.Diamond);
        Sprite tileSprite = CreateSprite("ground_tile", new Color(0.12f, 0.14f, 0.16f, 1f), SpriteShape.Square);
        Sprite swordSprite = CreateSprite("lightblade_sword", new Color(0.75f, 1f, 1f, 1f), SpriteShape.Diamond);
        Sprite crosshairSprite = CreateSprite("crosshair", new Color(0.9f, 1f, 1f, 1f), SpriteShape.Circle);
        UpgradeIconEntry[] upgradeIcons = CreateUpgradeIcons();
        Sprite walkerWoundedSprite = CreateSprite("walker_wounded", new Color(0.25f, 0.85f, 0.35f, 1f), SpriteShape.Circle);
        Sprite walkerCriticalSprite = CreateSprite("walker_critical", new Color(0.25f, 0.85f, 0.35f, 1f), SpriteShape.Circle);
        Sprite runnerWoundedSprite = CreateSprite("runner_wounded", new Color(1f, 0.45f, 0.25f, 1f), SpriteShape.Circle);
        Sprite runnerCriticalSprite = CreateSprite("runner_critical", new Color(1f, 0.45f, 0.25f, 1f), SpriteShape.Circle);
        Sprite spitterWoundedSprite = CreateSprite("spitter_wounded", new Color(0.95f, 0.85f, 0.25f, 1f), SpriteShape.Circle);
        Sprite spitterCriticalSprite = CreateSprite("spitter_critical", new Color(0.95f, 0.85f, 0.25f, 1f), SpriteShape.Circle);
        Sprite tankerWoundedSprite = CreateSprite("tanker_wounded", new Color(0.55f, 0.6f, 0.65f, 1f), SpriteShape.Circle);
        Sprite tankerCriticalSprite = CreateSprite("tanker_critical", new Color(0.55f, 0.6f, 0.65f, 1f), SpriteShape.Circle);
        Sprite bossWoundedSprite = CreateSprite("boss_wounded", new Color(0.9f, 0.15f, 0.25f, 1f), SpriteShape.Circle);
        Sprite bossCriticalSprite = CreateSprite("boss_critical", new Color(0.9f, 0.15f, 0.25f, 1f), SpriteShape.Circle);
        Sprite finalBossWoundedSprite = CreateSprite("final_boss_wounded", new Color(0.55f, 0.1f, 0.8f, 1f), SpriteShape.Circle);
        Sprite finalBossCriticalSprite = CreateSprite("final_boss_critical", new Color(0.55f, 0.1f, 0.8f, 1f), SpriteShape.Circle);

        GameObject bulletPrefab = CreateBulletPrefab(bulletSprite, projectileLayer);
        GameObject acidPrefab = CreateAcidPrefab(acidSprite, projectileLayer);
        GameObject orbPrefab = CreateSingularityOrbPrefab(orbSprite, projectileLayer);
        GameObject walkerPrefab = CreateEnemyPrefab("Walker", walkerSprite, walkerWoundedSprite, walkerCriticalSprite, enemyLayer, 0.75f, null);
        GameObject runnerPrefab = CreateEnemyPrefab("Runner", runnerSprite, runnerWoundedSprite, runnerCriticalSprite, enemyLayer, 0.58f, null);
        GameObject spitterPrefab = CreateEnemyPrefab("Spitter", spitterSprite, spitterWoundedSprite, spitterCriticalSprite, enemyLayer, 0.85f, acidPrefab);
        GameObject tankerPrefab = CreateEnemyPrefab("Tanker", tankerSprite, tankerWoundedSprite, tankerCriticalSprite, enemyLayer, 1.25f, null);
        GameObject mutantBossPrefab = CreateEnemyPrefab("MutantBoss", bossSprite, bossWoundedSprite, bossCriticalSprite, enemyLayer, 2.1f, null);
        GameObject finalBossPrefab = CreateEnemyPrefab("FinalBoss", finalBossSprite, finalBossWoundedSprite, finalBossCriticalSprite, enemyLayer, 2.7f, acidPrefab);
        GameObject xpPrefab = CreateExperiencePrefab(xpSprite, pickupLayer);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Main";

        InfiniteGround2D ground = CreateGround(tileSprite);

        GameObject player = CreatePlayer(playerSprite, playerLayer, pickupLayer, bulletPrefab, orbPrefab, enemyLayer, swordSprite);
        CameraFollow2D cameraFollow = CreateCamera(player.transform);
        ground.SetTarget(player.transform);
        CreateAimGuide(player.GetComponent<PlayerMovement>(), player.transform, crosshairSprite);
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
        SetFloatField(waveSpawner, "spawnDistance", 9.5f);
        SetIntField(waveSpawner, "openingBurstCount", 6);
        SetFloatField(waveSpawner, "openingBurstDistance", 7.2f);

        Canvas canvas = CreateCanvas();
        UpgradeIconLibrary iconLibrary = CreateIconLibrary(upgradeIcons);
        GameHud hud = CreateHud(canvas.transform);
        UpgradePanel upgradePanel = CreateUpgradePanel(canvas.transform, iconLibrary);
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
        SetObjectField(gameManager, "iconLibrary", iconLibrary);

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
        ValidateRuntimeSpawnSlice();
        Debug.Log("Zombie Overdrive smoke test completed.");
    }

    [MenuItem("Zombie Overdrive/Validate Runtime Spawn Slice")]
    public static void ValidateRuntimeSpawnSlice()
    {
        if (!File.Exists(ScenePath))
        {
            BuildPrototypeScene();
        }

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            throw new System.InvalidOperationException("Could not open prototype scene.");
        }

        GameManager manager = Object.FindObjectOfType<GameManager>(true);
        WaveSpawner spawner = Object.FindObjectOfType<WaveSpawner>(true);
        if (manager == null || spawner == null)
        {
            throw new System.InvalidOperationException("Runtime spawn validation failed: missing manager or spawner.");
        }

        spawner.Initialize(manager);
        System.Reflection.MethodInfo burstMethod = typeof(WaveSpawner).GetMethod("SpawnOpeningBurst", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (burstMethod == null)
        {
            throw new System.InvalidOperationException("Runtime spawn validation failed: missing SpawnOpeningBurst.");
        }

        burstMethod.Invoke(spawner, null);
        if (spawner.AliveEnemies <= 0)
        {
            throw new System.InvalidOperationException("Runtime spawn validation failed: no enemies spawned.");
        }

        Debug.Log("Zombie Overdrive runtime spawn validation passed. Alive enemies: " + spawner.AliveEnemies);
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
        RequireWaveSpawnerReferences();
        RequireObject<GameHud>("GameHud");
        RequireObject<UpgradePanel>("UpgradePanel");
        RequireObject<PauseMenu>("PauseMenu");
        RequireObject<UpgradeIconLibrary>("UpgradeIconLibrary");
        RequireObject<InfiniteGround2D>("InfiniteGround2D");
        RequireObject<AimGuide>("AimGuide");

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
        for (int i = 0; i < IconIds.Length; i++)
        {
            RequireAsset<Sprite>(ArtRoot + "/icon_" + IconIds[i] + ".png");
        }

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

    private static void RequireWaveSpawnerReferences()
    {
        WaveSpawner spawner = Object.FindObjectOfType<WaveSpawner>(true);
        if (spawner == null)
        {
            throw new System.InvalidOperationException("Missing scene object: WaveSpawner");
        }

        RequireSerializedReference(spawner, "walkerPool");
        RequireSerializedReference(spawner, "runnerPool");
        RequireSerializedReference(spawner, "spitterPool");
        RequireSerializedReference(spawner, "tankerPool");
        RequireSerializedReference(spawner, "mutantBossPool");
        RequireSerializedReference(spawner, "finalBossPool");
        RequireSerializedReference(spawner, "experiencePool");
    }

    private static void RequireSerializedReference(Object target, string fieldName)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(fieldName);
        if (property == null || property.objectReferenceValue == null)
        {
            throw new System.InvalidOperationException(target.name + " missing reference: " + fieldName);
        }
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
        Fill(texture, clear);

        if (!DrawNamedPixelSprite(texture, name))
        {
            DrawFallbackSprite(texture, color, shape);
        }

        texture.Apply(false);
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);

        AssetDatabase.ImportAsset(path);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 32f;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static UpgradeIconEntry[] CreateUpgradeIcons()
    {
        UpgradeIconEntry[] icons = new UpgradeIconEntry[IconIds.Length];
        for (int i = 0; i < IconIds.Length; i++)
        {
            icons[i] = new UpgradeIconEntry
            {
                Id = IconIds[i],
                Sprite = CreateSprite("icon_" + IconIds[i], Color.white, SpriteShape.Square)
            };
        }

        return icons;
    }

    private static UpgradeIconLibrary CreateIconLibrary(UpgradeIconEntry[] entries)
    {
        GameObject libraryObject = new GameObject("Upgrade Icon Library");
        UpgradeIconLibrary library = libraryObject.AddComponent<UpgradeIconLibrary>();
        SetIconEntries(library, entries);
        return library;
    }

    private static void SetIconEntries(UpgradeIconLibrary library, UpgradeIconEntry[] entries)
    {
        SerializedObject serializedObject = new SerializedObject(library);
        SerializedProperty property = serializedObject.FindProperty("icons");
        property.arraySize = entries.Length;
        for (int i = 0; i < entries.Length; i++)
        {
            SerializedProperty item = property.GetArrayElementAtIndex(i);
            item.FindPropertyRelative("Id").stringValue = entries[i].Id;
            item.FindPropertyRelative("Sprite").objectReferenceValue = entries[i].Sprite;
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(library);
    }

    private static bool DrawNamedPixelSprite(Texture2D texture, string name)
    {
        switch (name)
        {
            case "player":
                DrawPlayerSprite(texture);
                return true;
            case "walker":
                DrawWalkerSprite(texture);
                return true;
            case "walker_wounded":
                DrawWalkerSprite(texture);
                DrawDamageOverlay(texture, 1);
                return true;
            case "walker_critical":
                DrawWalkerSprite(texture);
                DrawDamageOverlay(texture, 2);
                return true;
            case "runner":
                DrawRunnerSprite(texture);
                return true;
            case "runner_wounded":
                DrawRunnerSprite(texture);
                DrawDamageOverlay(texture, 1);
                return true;
            case "runner_critical":
                DrawRunnerSprite(texture);
                DrawDamageOverlay(texture, 2);
                return true;
            case "spitter":
                DrawSpitterSprite(texture);
                return true;
            case "spitter_wounded":
                DrawSpitterSprite(texture);
                DrawDamageOverlay(texture, 1);
                return true;
            case "spitter_critical":
                DrawSpitterSprite(texture);
                DrawDamageOverlay(texture, 2);
                return true;
            case "tanker":
                DrawTankerSprite(texture);
                return true;
            case "tanker_wounded":
                DrawTankerSprite(texture);
                DrawDamageOverlay(texture, 1);
                return true;
            case "tanker_critical":
                DrawTankerSprite(texture);
                DrawDamageOverlay(texture, 2);
                return true;
            case "boss":
                DrawBossSprite(texture);
                return true;
            case "boss_wounded":
                DrawBossSprite(texture);
                DrawDamageOverlay(texture, 1);
                return true;
            case "boss_critical":
                DrawBossSprite(texture);
                DrawDamageOverlay(texture, 2);
                return true;
            case "final_boss":
                DrawFinalBossSprite(texture);
                return true;
            case "final_boss_wounded":
                DrawFinalBossSprite(texture);
                DrawDamageOverlay(texture, 1);
                return true;
            case "final_boss_critical":
                DrawFinalBossSprite(texture);
                DrawDamageOverlay(texture, 2);
                return true;
            case "bullet":
                DrawBulletSprite(texture);
                return true;
            case "acid":
                DrawAcidSprite(texture);
                return true;
            case "singularity_orb":
                DrawSingularitySprite(texture);
                return true;
            case "xp_crystal":
                DrawExperienceSprite(texture);
                return true;
            case "ground_tile":
                DrawGroundTileSprite(texture);
                return true;
            case "lightblade_sword":
                DrawSwordSprite(texture);
                return true;
            case "crosshair":
                DrawCrosshairSprite(texture);
                return true;
            default:
                if (name.StartsWith("icon_", System.StringComparison.Ordinal))
                {
                    DrawUpgradeIcon(texture, name.Substring(5));
                    return true;
                }

                return false;
        }
    }

    private static void DrawPlayerSprite(Texture2D texture)
    {
        Color outline = Hex("#172031");
        Color suit = Hex("#2cc9ff");
        Color suitDark = Hex("#1b6fb1");
        Color highlight = Hex("#8df1ff");
        Color skin = Hex("#f0b276");
        Color visor = Hex("#101d2a");
        Color metal = Hex("#b6c6d1");

        DrawOval(texture, 13, 8, 38, 11, WithAlpha(Hex("#05070a"), 0.35f));
        DrawRect(texture, 23, 10, 8, 10, outline);
        DrawRect(texture, 25, 12, 5, 7, suitDark);
        DrawRect(texture, 34, 10, 8, 10, outline);
        DrawRect(texture, 35, 12, 5, 7, suitDark);
        DrawRect(texture, 17, 20, 30, 21, outline);
        DrawRect(texture, 21, 23, 22, 16, suit);
        DrawRect(texture, 23, 34, 17, 4, highlight);
        DrawRect(texture, 14, 25, 8, 14, outline);
        DrawRect(texture, 16, 27, 5, 10, suitDark);
        DrawRect(texture, 42, 24, 9, 14, outline);
        DrawRect(texture, 44, 26, 6, 10, suitDark);
        DrawRect(texture, 20, 38, 24, 20, outline);
        DrawRect(texture, 23, 41, 18, 15, skin);
        DrawRect(texture, 22, 51, 20, 5, visor);
        DrawRect(texture, 26, 44, 4, 3, Hex("#f7d8a8"));
        DrawRect(texture, 35, 44, 4, 3, Hex("#f7d8a8"));
        DrawRect(texture, 47, 31, 13, 5, outline);
        DrawRect(texture, 50, 32, 9, 3, metal);
        DrawRect(texture, 58, 33, 4, 2, Hex("#ffe66a"));
        DrawRect(texture, 18, 23, 4, 10, Hex("#0d314f"));
    }

    private static void DrawWalkerSprite(Texture2D texture)
    {
        Color outline = Hex("#172414");
        Color skin = Hex("#5dbe4d");
        Color dark = Hex("#2f6b2f");
        Color cloth = Hex("#354b2e");
        Color wound = Hex("#b63f4c");

        DrawOval(texture, 14, 8, 38, 11, WithAlpha(Hex("#05070a"), 0.34f));
        DrawRect(texture, 20, 10, 9, 10, outline);
        DrawRect(texture, 22, 12, 5, 7, dark);
        DrawRect(texture, 35, 10, 9, 11, outline);
        DrawRect(texture, 37, 12, 5, 8, dark);
        DrawRect(texture, 17, 20, 30, 21, outline);
        DrawRect(texture, 21, 24, 22, 15, skin);
        DrawRect(texture, 23, 25, 17, 6, cloth);
        DrawRect(texture, 12, 27, 10, 8, outline);
        DrawRect(texture, 14, 29, 8, 4, skin);
        DrawRect(texture, 43, 27, 12, 8, outline);
        DrawRect(texture, 44, 29, 9, 4, skin);
        DrawRect(texture, 21, 39, 22, 19, outline);
        DrawRect(texture, 24, 42, 16, 14, skin);
        DrawRect(texture, 27, 51, 3, 2, Hex("#f7d95d"));
        DrawRect(texture, 35, 51, 3, 2, Hex("#f7d95d"));
        DrawRect(texture, 30, 44, 7, 2, dark);
        DrawRect(texture, 38, 34, 4, 5, wound);
        DrawRect(texture, 22, 36, 4, 3, Hex("#84e073"));
    }

    private static void DrawRunnerSprite(Texture2D texture)
    {
        Color outline = Hex("#2a1710");
        Color skin = Hex("#ff7542");
        Color dark = Hex("#a73725");
        Color cloth = Hex("#53322b");

        DrawOval(texture, 15, 8, 35, 10, WithAlpha(Hex("#05070a"), 0.32f));
        DrawRect(texture, 18, 11, 12, 8, outline);
        DrawRect(texture, 20, 13, 8, 4, dark);
        DrawRect(texture, 36, 11, 8, 12, outline);
        DrawRect(texture, 38, 13, 4, 9, dark);
        DrawRect(texture, 20, 20, 27, 20, outline);
        DrawRect(texture, 24, 23, 19, 15, skin);
        DrawRect(texture, 27, 25, 12, 5, cloth);
        DrawLine(texture, 15, 35, 24, 29, 4, outline);
        DrawLine(texture, 16, 35, 24, 30, 2, skin);
        DrawLine(texture, 43, 31, 54, 38, 4, outline);
        DrawLine(texture, 44, 31, 53, 37, 2, skin);
        DrawRect(texture, 24, 38, 20, 19, outline);
        DrawRect(texture, 28, 41, 13, 14, skin);
        DrawRect(texture, 30, 50, 3, 2, Hex("#ffd45c"));
        DrawRect(texture, 36, 50, 3, 2, Hex("#ffd45c"));
        DrawRect(texture, 24, 40, 6, 4, dark);
    }

    private static void DrawSpitterSprite(Texture2D texture)
    {
        Color outline = Hex("#26300f");
        Color skin = Hex("#d6d84c");
        Color acid = Hex("#a7ff2d");
        Color dark = Hex("#70851f");

        DrawOval(texture, 14, 9, 40, 12, WithAlpha(Hex("#05070a"), 0.34f));
        DrawRect(texture, 21, 13, 9, 13, outline);
        DrawRect(texture, 23, 15, 5, 10, dark);
        DrawRect(texture, 35, 13, 9, 13, outline);
        DrawRect(texture, 37, 15, 5, 10, dark);
        DrawRect(texture, 18, 22, 29, 25, outline);
        DrawRect(texture, 22, 26, 21, 17, skin);
        DrawRect(texture, 27, 28, 11, 10, acid);
        DrawRect(texture, 29, 30, 7, 6, Hex("#e8ff74"));
        DrawRect(texture, 13, 31, 10, 8, outline);
        DrawRect(texture, 15, 33, 7, 4, skin);
        DrawRect(texture, 43, 31, 9, 8, outline);
        DrawRect(texture, 44, 33, 7, 4, skin);
        DrawRect(texture, 24, 43, 18, 14, outline);
        DrawRect(texture, 27, 45, 12, 10, skin);
        DrawRect(texture, 30, 46, 7, 3, acid);
        DrawRect(texture, 28, 52, 2, 2, Hex("#fff48a"));
        DrawRect(texture, 36, 52, 2, 2, Hex("#fff48a"));
        DrawRect(texture, 39, 40, 4, 4, acid);
    }

    private static void DrawTankerSprite(Texture2D texture)
    {
        Color outline = Hex("#1d2328");
        Color armor = Hex("#8a99a6");
        Color armorDark = Hex("#4e5d67");
        Color skin = Hex("#5dbb5d");
        Color rust = Hex("#9b6a38");

        DrawOval(texture, 10, 8, 46, 13, WithAlpha(Hex("#05070a"), 0.42f));
        DrawRect(texture, 17, 11, 11, 15, outline);
        DrawRect(texture, 20, 14, 6, 11, armorDark);
        DrawRect(texture, 37, 11, 11, 15, outline);
        DrawRect(texture, 39, 14, 6, 11, armorDark);
        DrawRect(texture, 14, 22, 36, 29, outline);
        DrawRect(texture, 18, 25, 28, 23, armor);
        DrawRect(texture, 21, 28, 9, 17, armorDark);
        DrawRect(texture, 34, 28, 9, 17, armorDark);
        DrawRect(texture, 12, 31, 8, 15, outline);
        DrawRect(texture, 14, 33, 5, 11, skin);
        DrawRect(texture, 47, 31, 8, 15, outline);
        DrawRect(texture, 48, 33, 5, 11, skin);
        DrawRect(texture, 23, 47, 19, 12, outline);
        DrawRect(texture, 26, 49, 13, 8, skin);
        DrawRect(texture, 28, 54, 2, 2, Hex("#f2e56c"));
        DrawRect(texture, 36, 54, 2, 2, Hex("#f2e56c"));
        DrawRect(texture, 44, 38, 4, 4, rust);
        DrawRect(texture, 18, 44, 4, 3, Hex("#c1ccd3"));
    }

    private static void DrawBossSprite(Texture2D texture)
    {
        Color outline = Hex("#2c0e18");
        Color flesh = Hex("#d9384e");
        Color fleshDark = Hex("#8a1f38");
        Color eye = Hex("#ffe36b");
        Color claw = Hex("#e8d4b4");

        DrawOval(texture, 7, 7, 51, 14, WithAlpha(Hex("#05070a"), 0.48f));
        DrawRect(texture, 12, 28, 10, 18, outline);
        DrawRect(texture, 14, 30, 6, 14, fleshDark);
        DrawRect(texture, 43, 28, 10, 18, outline);
        DrawRect(texture, 45, 30, 6, 14, fleshDark);
        DrawRect(texture, 16, 16, 33, 36, outline);
        DrawRect(texture, 20, 20, 25, 28, flesh);
        DrawRect(texture, 24, 24, 17, 12, fleshDark);
        DrawRect(texture, 21, 49, 22, 10, outline);
        DrawRect(texture, 24, 51, 16, 6, flesh);
        DrawRect(texture, 25, 54, 3, 2, eye);
        DrawRect(texture, 31, 54, 3, 2, eye);
        DrawRect(texture, 37, 54, 3, 2, eye);
        DrawRect(texture, 9, 42, 8, 4, claw);
        DrawRect(texture, 48, 42, 8, 4, claw);
        DrawRect(texture, 30, 37, 5, 7, Hex("#ff7682"));
        DrawRect(texture, 41, 26, 4, 5, Hex("#652037"));
    }

    private static void DrawFinalBossSprite(Texture2D texture)
    {
        Color outline = Hex("#130d22");
        Color shell = Hex("#6f2bbf");
        Color dark = Hex("#2d1856");
        Color core = Hex("#ff4c7b");
        Color eye = Hex("#fff067");

        DrawOval(texture, 5, 6, 54, 16, WithAlpha(Hex("#05070a"), 0.55f));
        DrawRect(texture, 9, 24, 10, 22, outline);
        DrawRect(texture, 11, 27, 6, 17, dark);
        DrawRect(texture, 45, 24, 10, 22, outline);
        DrawRect(texture, 47, 27, 6, 17, dark);
        DrawRect(texture, 14, 15, 36, 38, outline);
        DrawRect(texture, 18, 19, 28, 30, shell);
        DrawRect(texture, 22, 23, 20, 18, dark);
        DrawDiamond(texture, 32, 32, 9, core);
        DrawDiamond(texture, 32, 32, 4, Hex("#ff9eba"));
        DrawRect(texture, 20, 50, 24, 10, outline);
        DrawRect(texture, 23, 52, 18, 6, shell);
        DrawRect(texture, 25, 55, 3, 2, eye);
        DrawRect(texture, 31, 55, 3, 2, eye);
        DrawRect(texture, 37, 55, 3, 2, eye);
        DrawLine(texture, 8, 45, 2, 52, 3, Hex("#caa7ff"));
        DrawLine(texture, 56, 45, 62, 52, 3, Hex("#caa7ff"));
        DrawRect(texture, 16, 42, 5, 5, core);
        DrawRect(texture, 43, 42, 5, 5, core);
    }

    private static void DrawBulletSprite(Texture2D texture)
    {
        Color outline = Hex("#5a330d");
        Color orange = Hex("#ff9f1c");
        Color yellow = Hex("#ffe66d");

        DrawRect(texture, 9, 27, 39, 10, outline);
        DrawRect(texture, 13, 29, 31, 6, orange);
        DrawRect(texture, 40, 30, 10, 4, yellow);
        DrawDiamond(texture, 52, 32, 6, yellow);
        DrawRect(texture, 5, 30, 8, 4, Hex("#ff5a1f"));
        DrawRect(texture, 18, 34, 16, 2, Hex("#fff3a1"));
    }

    private static void DrawAcidSprite(Texture2D texture)
    {
        Color outline = Hex("#315e12");
        Color acid = Hex("#a9ff25");
        Color light = Hex("#e6ff72");

        DrawDiamond(texture, 32, 32, 24, outline);
        DrawDiamond(texture, 32, 32, 19, acid);
        DrawDiamond(texture, 27, 38, 6, light);
        DrawRect(texture, 42, 22, 6, 4, Hex("#72cf1d"));
        DrawRect(texture, 16, 25, 5, 5, acid);
        DrawRect(texture, 47, 43, 4, 4, light);
    }

    private static void DrawSingularitySprite(Texture2D texture)
    {
        Color outline = Hex("#160d2c");
        Color purple = Hex("#5e35d9");
        Color blue = Hex("#28c7ff");

        DrawCircle(texture, 32, 32, 25, outline);
        DrawCircle(texture, 32, 32, 20, purple);
        DrawCircle(texture, 32, 32, 10, Hex("#0b1022"));
        DrawRing(texture, 32, 32, 14, 16, blue);
        DrawLine(texture, 19, 35, 44, 27, 3, Hex("#b88cff"));
        DrawLine(texture, 23, 22, 42, 42, 2, Hex("#78e7ff"));
        DrawRect(texture, 29, 29, 6, 6, Hex("#02040a"));
    }

    private static void DrawExperienceSprite(Texture2D texture)
    {
        Color outline = Hex("#092d66");
        Color blue = Hex("#2d7dff");
        Color light = Hex("#9ce7ff");

        DrawDiamond(texture, 32, 32, 25, outline);
        DrawDiamond(texture, 32, 32, 20, blue);
        DrawLine(texture, 32, 12, 32, 52, 2, Hex("#1456c4"));
        DrawLine(texture, 18, 32, 46, 32, 2, Hex("#1456c4"));
        DrawRect(texture, 26, 41, 10, 5, light);
        DrawRect(texture, 38, 35, 4, 4, light);
    }

    private static void DrawGroundTileSprite(Texture2D texture)
    {
        Color baseColor = Hex("#171c20");
        Color alternate = Hex("#1d2429");
        Color crack = Hex("#0b0f12");
        Color dust = Hex("#2a3338");
        Color moss = Hex("#30482e");

        Fill(texture, baseColor);
        DrawRect(texture, 0, 0, 64, 3, Hex("#101418"));
        DrawRect(texture, 0, 61, 64, 3, Hex("#20272d"));
        DrawRect(texture, 0, 0, 3, 64, Hex("#101418"));
        DrawRect(texture, 61, 0, 3, 64, Hex("#20272d"));

        DrawRect(texture, 7, 9, 10, 5, alternate);
        DrawRect(texture, 40, 6, 12, 7, alternate);
        DrawRect(texture, 25, 27, 14, 9, alternate);
        DrawRect(texture, 8, 48, 17, 6, alternate);
        DrawRect(texture, 47, 43, 8, 12, alternate);
        DrawLine(texture, 12, 28, 24, 32, 2, crack);
        DrawLine(texture, 24, 32, 31, 42, 2, crack);
        DrawLine(texture, 44, 18, 52, 25, 2, crack);
        DrawLine(texture, 52, 25, 58, 25, 2, crack);
        DrawRect(texture, 15, 16, 3, 2, dust);
        DrawRect(texture, 35, 19, 2, 3, dust);
        DrawRect(texture, 53, 53, 3, 2, dust);
        DrawRect(texture, 5, 37, 2, 2, moss);
        DrawRect(texture, 34, 50, 6, 3, moss);
        DrawRect(texture, 49, 11, 3, 3, moss);
    }

    private static void DrawSwordSprite(Texture2D texture)
    {
        Color outline = Hex("#103042");
        Color blade = Hex("#bffcff");
        Color core = Hex("#ffffff");
        Color hilt = Hex("#24305a");
        Color grip = Hex("#ffd36b");

        DrawLine(texture, 18, 8, 42, 54, 8, outline);
        DrawLine(texture, 20, 10, 43, 54, 5, blade);
        DrawLine(texture, 23, 16, 43, 54, 2, core);
        DrawRect(texture, 15, 10, 18, 5, hilt);
        DrawRect(texture, 21, 3, 5, 12, grip);
        DrawDiamond(texture, 44, 56, 5, blade);
    }

    private static void DrawCrosshairSprite(Texture2D texture)
    {
        Color color = Hex("#d9fbff");
        Color shadow = WithAlpha(Hex("#0a1d24"), 0.65f);

        DrawRing(texture, 32, 32, 11, 12, shadow);
        DrawRing(texture, 32, 32, 9, 10, color);
        DrawRect(texture, 31, 13, 2, 10, color);
        DrawRect(texture, 31, 41, 2, 10, color);
        DrawRect(texture, 13, 31, 10, 2, color);
        DrawRect(texture, 41, 31, 10, 2, color);
        DrawRect(texture, 31, 31, 2, 2, WithAlpha(color, 0.7f));
    }

    private static void DrawDamageOverlay(Texture2D texture, int severity)
    {
        Color blood = severity >= 2 ? Hex("#7e1020") : Hex("#b82a32");
        Color darkBlood = Hex("#3c0610");
        DrawRect(texture, 22, 30, 5, 5, blood);
        DrawRect(texture, 40, 36, 4, 5, blood);
        DrawRect(texture, 31, 45, 8, 3, blood);

        if (severity < 2)
        {
            return;
        }

        DrawRect(texture, 25, 50, 4, 3, blood);
        DrawRect(texture, 43, 27, 5, 4, darkBlood);
        DrawLine(texture, 20, 23, 27, 19, 2, darkBlood);
        DrawLine(texture, 35, 40, 45, 46, 2, darkBlood);
        DrawRect(texture, 16, 35, 3, 3, blood);
    }

    private static void DrawUpgradeIcon(Texture2D texture, string id)
    {
        Color background = IconBackground(id);
        Color dark = Darken(background, 0.55f);
        Color light = Lighten(background, 0.55f);
        Color white = Hex("#f7fbff");
        Color red = Hex("#d92d3a");
        Color black = Hex("#151820");

        Fill(texture, new Color(0f, 0f, 0f, 0f));
        DrawRect(texture, 9, 9, 46, 46, Hex("#10141d"));
        DrawRect(texture, 11, 11, 42, 42, background);
        DrawRect(texture, 11, 11, 42, 7, light);
        DrawRect(texture, 11, 47, 42, 6, dark);
        DrawLine(texture, 13, 51, 51, 13, 3, WithAlpha(white, 0.16f));

        if (id.StartsWith("evolution_", System.StringComparison.Ordinal))
        {
            DrawDiamond(texture, 32, 32, 26, Hex("#3b1022"));
            DrawDiamond(texture, 32, 32, 22, background);
            DrawRect(texture, 14, 14, 36, 5, white);
            DrawRect(texture, 14, 45, 36, 5, dark);
            id = "weapon_" + id.Substring(10);
        }

        switch (id)
        {
            case "weapon_pistol":
                DrawLine(texture, 20, 38, 45, 24, 7, black);
                DrawLine(texture, 22, 37, 45, 25, 4, white);
                DrawRect(texture, 18, 36, 9, 12, black);
                DrawRect(texture, 20, 37, 5, 9, dark);
                DrawRect(texture, 44, 22, 8, 4, Hex("#ffd15d"));
                break;
            case "weapon_shotgun":
                DrawLine(texture, 14, 42, 50, 23, 8, black);
                DrawLine(texture, 17, 40, 50, 24, 5, white);
                DrawRect(texture, 16, 39, 11, 8, dark);
                DrawLine(texture, 40, 25, 54, 17, 3, Hex("#ffca55"));
                break;
            case "weapon_tesla":
                DrawLine(texture, 19, 47, 27, 20, 6, black);
                DrawLine(texture, 22, 46, 29, 22, 3, white);
                DrawLine(texture, 30, 20, 24, 34, 4, Hex("#80f7ff"));
                DrawLine(texture, 24, 34, 38, 30, 4, Hex("#80f7ff"));
                DrawLine(texture, 38, 30, 30, 48, 4, Hex("#80f7ff"));
                break;
            case "weapon_singularity":
                DrawCircle(texture, 32, 32, 17, black);
                DrawRing(texture, 32, 32, 19, 21, white);
                DrawCircle(texture, 32, 32, 8, Hex("#000000"));
                DrawLine(texture, 16, 36, 47, 25, 3, Hex("#b895ff"));
                break;
            case "weapon_lightblade":
                DrawLine(texture, 17, 45, 45, 18, 8, black);
                DrawLine(texture, 20, 43, 45, 19, 5, white);
                DrawLine(texture, 17, 45, 28, 53, 5, Hex("#ffd86b"));
                DrawRing(texture, 32, 32, 22, 24, WithAlpha(white, 0.45f));
                break;
            case "weapon_laser":
                DrawLine(texture, 14, 43, 45, 22, 8, black);
                DrawLine(texture, 18, 41, 46, 23, 4, white);
                DrawLine(texture, 31, 32, 55, 16, 4, Hex("#fff16b"));
                DrawLine(texture, 31, 32, 55, 16, 2, red);
                break;
            case "passive_ammobox":
                DrawRect(texture, 17, 24, 31, 23, black);
                DrawRect(texture, 20, 27, 25, 17, Hex("#f2c45d"));
                DrawRect(texture, 24, 18, 16, 8, black);
                DrawRect(texture, 26, 20, 12, 5, light);
                DrawRect(texture, 28, 31, 9, 8, red);
                break;
            case "passive_overclock":
                DrawRing(texture, 32, 32, 17, 20, white);
                DrawLine(texture, 32, 32, 44, 24, 4, black);
                DrawLine(texture, 32, 32, 28, 19, 3, black);
                DrawRect(texture, 29, 29, 6, 6, red);
                break;
            case "passive_adrenaline":
                DrawLine(texture, 18, 45, 47, 16, 7, white);
                DrawLine(texture, 18, 45, 47, 16, 3, red);
                DrawRect(texture, 42, 12, 8, 8, black);
                DrawLine(texture, 15, 35, 27, 35, 3, white);
                DrawLine(texture, 27, 35, 34, 27, 3, white);
                break;
            case "passive_nanoarmor":
                DrawDiamond(texture, 32, 34, 22, black);
                DrawDiamond(texture, 32, 34, 17, Hex("#c5d1d9"));
                DrawRect(texture, 25, 24, 14, 20, dark);
                DrawLine(texture, 20, 34, 44, 34, 3, white);
                break;
            case "passive_propellent":
                DrawRect(texture, 25, 15, 16, 34, black);
                DrawRect(texture, 28, 18, 10, 26, white);
                DrawDiamond(texture, 33, 50, 10, Hex("#ff8b2d"));
                DrawLine(texture, 21, 30, 45, 30, 2, red);
                break;
            case "passive_gravitycore":
                DrawCircle(texture, 32, 32, 16, black);
                DrawCircle(texture, 32, 32, 9, Hex("#8c5cff"));
                DrawRing(texture, 32, 32, 21, 23, white);
                DrawDiamond(texture, 32, 32, 4, white);
                break;
            case "passive_magnet":
                DrawRect(texture, 18, 18, 10, 25, red);
                DrawRect(texture, 38, 18, 10, 25, red);
                DrawRect(texture, 18, 18, 30, 8, white);
                DrawRect(texture, 26, 37, 14, 7, black);
                break;
            case "passive_hazmatsuit":
                DrawCircle(texture, 32, 34, 19, black);
                DrawCircle(texture, 32, 34, 15, Hex("#e3d86a"));
                DrawCircle(texture, 27, 37, 4, white);
                DrawCircle(texture, 37, 37, 4, white);
                DrawRect(texture, 25, 23, 14, 4, dark);
                break;
            case "passive_greedchip":
                DrawRect(texture, 18, 18, 28, 28, black);
                DrawRect(texture, 22, 22, 20, 20, Hex("#5cff8b"));
                DrawRect(texture, 29, 25, 7, 15, dark);
                DrawLine(texture, 16, 27, 10, 27, 2, white);
                DrawLine(texture, 48, 37, 55, 37, 2, white);
                break;
            case "passive_radar":
                DrawRing(texture, 32, 32, 6, 8, white);
                DrawRing(texture, 32, 32, 15, 17, WithAlpha(white, 0.65f));
                DrawRing(texture, 32, 32, 24, 25, WithAlpha(white, 0.4f));
                DrawLine(texture, 32, 32, 50, 18, 3, red);
                break;
            case "passive_defibrillator":
                DrawRect(texture, 17, 25, 13, 20, white);
                DrawRect(texture, 35, 25, 13, 20, white);
                DrawLine(texture, 24, 25, 32, 17, 3, black);
                DrawLine(texture, 41, 25, 32, 17, 3, black);
                DrawLine(texture, 24, 35, 30, 35, 3, red);
                DrawLine(texture, 27, 32, 27, 38, 3, red);
                break;
            case "passive_radio":
                DrawRect(texture, 20, 24, 26, 22, black);
                DrawRect(texture, 23, 27, 20, 16, Hex("#dce6ea"));
                DrawLine(texture, 42, 24, 51, 13, 3, white);
                DrawCircle(texture, 29, 35, 4, dark);
                DrawRect(texture, 35, 31, 5, 8, red);
                break;
            case "passive_repair":
                DrawRect(texture, 18, 27, 28, 12, white);
                DrawRect(texture, 26, 19, 12, 28, white);
                DrawRect(texture, 22, 31, 20, 4, red);
                DrawRect(texture, 30, 23, 4, 20, red);
                break;
            default:
                DrawDiamond(texture, 32, 32, 18, white);
                break;
        }

        DrawRect(texture, 9, 9, 46, 2, WithAlpha(white, 0.45f));
        DrawRect(texture, 9, 53, 46, 2, WithAlpha(black, 0.35f));
    }

    private static void DrawFallbackSprite(Texture2D texture, Color color, SpriteShape shape)
    {
        const int size = 64;
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

                if (fill)
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }
    }

    private static void Fill(Texture2D texture, Color color)
    {
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, color);
            }
        }
    }

    private static void DrawRect(Texture2D texture, int x, int y, int width, int height, Color color)
    {
        for (int py = y; py < y + height; py++)
        {
            for (int px = x; px < x + width; px++)
            {
                SetPixel(texture, px, py, color);
            }
        }
    }

    private static void DrawCircle(Texture2D texture, int centerX, int centerY, int radius, Color color)
    {
        int radiusSquared = radius * radius;
        for (int y = centerY - radius; y <= centerY + radius; y++)
        {
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                int dx = x - centerX;
                int dy = y - centerY;
                if (dx * dx + dy * dy <= radiusSquared)
                {
                    SetPixel(texture, x, y, color);
                }
            }
        }
    }

    private static void DrawRing(Texture2D texture, int centerX, int centerY, int innerRadius, int outerRadius, Color color)
    {
        int innerSquared = innerRadius * innerRadius;
        int outerSquared = outerRadius * outerRadius;
        for (int y = centerY - outerRadius; y <= centerY + outerRadius; y++)
        {
            for (int x = centerX - outerRadius; x <= centerX + outerRadius; x++)
            {
                int dx = x - centerX;
                int dy = y - centerY;
                int distance = dx * dx + dy * dy;
                if (distance >= innerSquared && distance <= outerSquared)
                {
                    SetPixel(texture, x, y, color);
                }
            }
        }
    }

    private static void DrawDiamond(Texture2D texture, int centerX, int centerY, int radius, Color color)
    {
        for (int y = centerY - radius; y <= centerY + radius; y++)
        {
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                if (Mathf.Abs(x - centerX) + Mathf.Abs(y - centerY) <= radius)
                {
                    SetPixel(texture, x, y, color);
                }
            }
        }
    }

    private static void DrawOval(Texture2D texture, int x, int y, int width, int height, Color color)
    {
        float radiusX = width * 0.5f;
        float radiusY = height * 0.5f;
        float centerX = x + radiusX;
        float centerY = y + radiusY;

        for (int py = y; py < y + height; py++)
        {
            for (int px = x; px < x + width; px++)
            {
                float normalizedX = (px - centerX) / radiusX;
                float normalizedY = (py - centerY) / radiusY;
                if (normalizedX * normalizedX + normalizedY * normalizedY <= 1f)
                {
                    SetPixel(texture, px, py, color);
                }
            }
        }
    }

    private static void DrawLine(Texture2D texture, int x0, int y0, int x1, int y1, int thickness, Color color)
    {
        int dx = Mathf.Abs(x1 - x0);
        int sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0);
        int sy = y0 < y1 ? 1 : -1;
        int error = dx + dy;

        while (true)
        {
            DrawRect(texture, x0 - thickness / 2, y0 - thickness / 2, thickness, thickness, color);
            if (x0 == x1 && y0 == y1)
            {
                break;
            }

            int doubledError = 2 * error;
            if (doubledError >= dy)
            {
                error += dy;
                x0 += sx;
            }

            if (doubledError <= dx)
            {
                error += dx;
                y0 += sy;
            }
        }
    }

    private static void SetPixel(Texture2D texture, int x, int y, Color color)
    {
        if (x < 0 || x >= texture.width || y < 0 || y >= texture.height)
        {
            return;
        }

        texture.SetPixel(x, y, color);
    }

    private static Color Hex(string value)
    {
        Color color;
        return ColorUtility.TryParseHtmlString(value, out color) ? color : Color.magenta;
    }

    private static Color IconBackground(string id)
    {
        if (id.StartsWith("evolution_", System.StringComparison.Ordinal))
        {
            return Hex("#b91c3d");
        }

        if (id.StartsWith("weapon_", System.StringComparison.Ordinal))
        {
            if (id.Contains("tesla"))
            {
                return Hex("#186d8b");
            }

            if (id.Contains("singularity"))
            {
                return Hex("#4c2aa5");
            }

            if (id.Contains("lightblade"))
            {
                return Hex("#1c8a78");
            }

            if (id.Contains("laser"))
            {
                return Hex("#b45309");
            }

            return Hex("#9f2538");
        }

        if (id.Contains("armor") || id.Contains("hazmat"))
        {
            return Hex("#56616f");
        }

        if (id.Contains("magnet") || id.Contains("gravity"))
        {
            return Hex("#5745a8");
        }

        if (id.Contains("radio") || id.Contains("radar"))
        {
            return Hex("#0f766e");
        }

        if (id.Contains("repair") || id.Contains("defibrillator"))
        {
            return Hex("#047857");
        }

        return Hex("#9f3522");
    }

    private static Color Darken(Color color, float amount)
    {
        return Color.Lerp(color, Color.black, Mathf.Clamp01(amount));
    }

    private static Color Lighten(Color color, float amount)
    {
        return Color.Lerp(color, Color.white, Mathf.Clamp01(amount));
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
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

    private static GameObject CreateEnemyPrefab(string name, Sprite sprite, Sprite woundedSprite, Sprite criticalSprite, int layer, float scale, GameObject acidProjectilePrefab)
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
        EnemyDamageVisual damageVisual = enemy.AddComponent<EnemyDamageVisual>();
        damageVisual.Configure(sprite, woundedSprite, criticalSprite);
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

    private static GameObject CreatePlayer(Sprite sprite, int playerLayer, int pickupLayer, GameObject bulletPrefab, GameObject orbPrefab, int enemyLayer, Sprite swordSprite)
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
        SetObjectField(lightblade, "swordSprite", swordSprite);
        LaserWeapon laser = player.AddComponent<LaserWeapon>();
        SetLayerMask(laser, "enemyMask", 1 << enemyLayer);

        return player;
    }

    private static InfiniteGround2D CreateGround(Sprite sprite)
    {
        GameObject root = new GameObject("Infinite Ground Placeholder");
        InfiniteGround2D infiniteGround = root.AddComponent<InfiniteGround2D>();
        const float tileSize = 36f;
        SetFloatField(infiniteGround, "tileSize", tileSize);
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
                renderer.color = Color.white;
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

    private static AimGuide CreateAimGuide(PlayerMovement movement, Transform player, Sprite crosshairSprite)
    {
        GameObject guideObject = new GameObject("Aim Guide");
        guideObject.AddComponent<LineRenderer>();
        AimGuide guide = guideObject.AddComponent<AimGuide>();
        GameObject crosshairObject = new GameObject("Mouse Crosshair");
        SpriteRenderer renderer = crosshairObject.AddComponent<SpriteRenderer>();
        renderer.sprite = crosshairSprite;
        renderer.sortingOrder = 15;
        crosshairObject.transform.localScale = Vector3.one * 0.65f;
        guide.Initialize(movement, player, renderer);
        return guide;
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

        Image statusPanel = CreateImage(hudObject.transform, "Status Panel", new Vector2(0f, 1f), new Vector2(18f, -18f), new Vector2(318f, 122f));
        statusPanel.color = new Color(0.025f, 0.03f, 0.042f, 0.78f);
        Image timerPanel = CreateImage(hudObject.transform, "Timer Panel", new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(250f, 70f));
        timerPanel.color = new Color(0.025f, 0.03f, 0.042f, 0.82f);
        Image killPanel = CreateImage(hudObject.transform, "Kill Panel", new Vector2(1f, 1f), new Vector2(-18f, -18f), new Vector2(226f, 58f));
        killPanel.color = new Color(0.025f, 0.03f, 0.042f, 0.78f);
        Image xpPanel = CreateImage(hudObject.transform, "XP Panel", new Vector2(0.5f, 0f), new Vector2(0f, 16f), new Vector2(840f, 38f));
        xpPanel.color = new Color(0.025f, 0.03f, 0.042f, 0.72f);

        Text timer = CreateText(hudObject.transform, "Timer", "10:00", 42, TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(220f, 60f));
        Text health = CreateText(hudObject.transform, "Health Text", "生命", 23, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(34f, -24f), new Vector2(280f, 38f));
        Text level = CreateText(hudObject.transform, "Level Text", "等级 1", 23, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(34f, -92f), new Vector2(160f, 36f));
        Text kills = CreateText(hudObject.transform, "Kills Text", "击杀 0", 23, TextAnchor.UpperRight, new Vector2(1f, 1f), new Vector2(-32f, -28f), new Vector2(190f, 40f));
        Text message = CreateText(hudObject.transform, "Message", "", 48, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 120f));

        Slider hpSlider = CreateSlider(hudObject.transform, "Health Bar", new Vector2(0f, 1f), new Vector2(34f, -62f), new Vector2(270f, 20f), new Color(0.85f, 0.1f, 0.12f, 1f));
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

    private static UpgradePanel CreateUpgradePanel(Transform parent, UpgradeIconLibrary iconLibrary)
    {
        GameObject panel = new GameObject("Upgrade Panel");
        panel.transform.SetParent(parent, false);
        Image background = panel.AddComponent<Image>();
        background.color = new Color(0.025f, 0.03f, 0.042f, 0.96f);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(1040f, 560f);

        CreateText(panel.transform, "Title", "选择升级", 42, TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(600f, 70f));

        UpgradePanel upgradePanel = panel.AddComponent<UpgradePanel>();
        Button[] buttons = new Button[3];
        Image[] icons = new Image[3];
        Image[] accents = new Image[3];
        Text[] titles = new Text[3];
        Text[] descriptions = new Text[3];
        Text[] hints = new Text[3];

        for (int i = 0; i < 3; i++)
        {
            GameObject buttonObject = new GameObject("Option " + (i + 1));
            buttonObject.transform.SetParent(panel.transform, false);
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.1f, 0.12f, 0.16f, 0.98f);
            Button button = buttonObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.17f, 0.22f, 0.3f, 1f);
            colors.pressedColor = new Color(0.07f, 0.085f, 0.12f, 1f);
            colors.selectedColor = new Color(0.17f, 0.22f, 0.3f, 1f);
            button.colors = colors;

            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = new Vector2((i - 1) * 320f, -42f);
            buttonRect.sizeDelta = new Vector2(286f, 354f);

            Image accent = CreateImage(buttonObject.transform, "Accent", new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(286f, 8f));
            accent.color = new Color(0.28f, 0.56f, 0.78f, 1f);
            accents[i] = accent;

            Image iconBackdrop = CreateImage(buttonObject.transform, "Icon Backdrop", new Vector2(0.5f, 1f), new Vector2(0f, -92f), new Vector2(98f, 98f));
            iconBackdrop.color = new Color(0.03f, 0.035f, 0.05f, 0.92f);
            icons[i] = CreateImage(buttonObject.transform, "Icon", new Vector2(0.5f, 1f), new Vector2(0f, -92f), new Vector2(84f, 84f));
            icons[i].preserveAspect = true;

            titles[i] = CreateText(buttonObject.transform, "Title", "升级", 23, TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0f, -22f), new Vector2(250f, 56f));
            descriptions[i] = CreateText(buttonObject.transform, "Description", "说明", 19, TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0f, -176f), new Vector2(242f, 98f));
            hints[i] = CreateText(buttonObject.transform, "Hint", "", 17, TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(242f, 44f));
            hints[i].color = new Color(1f, 0.86f, 0.48f, 1f);
            buttons[i] = button;
        }

        SetArrayField(upgradePanel, "optionButtons", buttons);
        SetArrayField(upgradePanel, "iconImages", icons);
        SetArrayField(upgradePanel, "accentImages", accents);
        SetArrayField(upgradePanel, "titleTexts", titles);
        SetArrayField(upgradePanel, "descriptionTexts", descriptions);
        SetArrayField(upgradePanel, "hintTexts", hints);
        SetObjectField(upgradePanel, "iconLibrary", iconLibrary);
        panel.SetActive(false);
        return upgradePanel;
    }

    private static PauseMenu CreatePauseMenu(Transform parent)
    {
        GameObject panel = new GameObject("Pause Menu");
        panel.transform.SetParent(parent, false);
        Image background = panel.AddComponent<Image>();
        background.color = new Color(0.025f, 0.03f, 0.042f, 0.96f);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(1040f, 660f);

        CreateText(panel.transform, "Title", "暂停", 44, TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(500f, 70f));
        CreateText(panel.transform, "Active Slots Title", "主动武器", 24, TextAnchor.UpperLeft, new Vector2(0.5f, 1f), new Vector2(-455f, -92f), new Vector2(330f, 38f));
        CreateText(panel.transform, "Passive Slots Title", "被动技能", 24, TextAnchor.UpperLeft, new Vector2(0.5f, 1f), new Vector2(-455f, -306f), new Vector2(330f, 38f));
        Text status = CreateText(panel.transform, "Status", "", 17, TextAnchor.UpperLeft, new Vector2(0.5f, 1f), new Vector2(70f, -92f), new Vector2(500f, 430f));
        status.color = new Color(0.86f, 0.9f, 0.96f, 1f);

        Image[] activeIcons = new Image[3];
        Text[] activeTexts = new Text[3];
        Image[] passiveIcons = new Image[3];
        Text[] passiveTexts = new Text[3];
        for (int i = 0; i < 3; i++)
        {
            CreateSlotItem(panel.transform, "Active Slot " + (i + 1), new Vector2(-350f, -132f - i * 62f), out activeIcons[i], out activeTexts[i]);
            CreateSlotItem(panel.transform, "Passive Slot " + (i + 1), new Vector2(-350f, -346f - i * 62f), out passiveIcons[i], out passiveTexts[i]);
        }

        Button resume = CreateMenuButton(panel.transform, "Resume Button", "继续", new Vector2(-220f, -285f));
        Button restart = CreateMenuButton(panel.transform, "Restart Button", "重开", new Vector2(0f, -285f));
        Button quit = CreateMenuButton(panel.transform, "Quit Button", "退出", new Vector2(220f, -285f));

        PauseMenu pauseMenu = panel.AddComponent<PauseMenu>();
        SetObjectField(pauseMenu, "statusText", status);
        SetArrayField(pauseMenu, "activeSlotIcons", activeIcons);
        SetArrayField(pauseMenu, "activeSlotTexts", activeTexts);
        SetArrayField(pauseMenu, "passiveSlotIcons", passiveIcons);
        SetArrayField(pauseMenu, "passiveSlotTexts", passiveTexts);
        SetObjectField(pauseMenu, "resumeButton", resume);
        SetObjectField(pauseMenu, "restartButton", restart);
        SetObjectField(pauseMenu, "quitButton", quit);
        panel.SetActive(false);
        return pauseMenu;
    }

    private static void CreateSlotItem(Transform parent, string name, Vector2 position, out Image icon, out Text label)
    {
        GameObject slotObject = new GameObject(name);
        slotObject.transform.SetParent(parent, false);
        Image background = slotObject.AddComponent<Image>();
        background.color = new Color(0.075f, 0.09f, 0.12f, 0.9f);
        RectTransform rect = slotObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(330f, 52f);

        Image iconBackdrop = CreateImage(slotObject.transform, "Icon Backdrop", new Vector2(0f, 0.5f), new Vector2(12f, 0f), new Vector2(40f, 40f));
        iconBackdrop.color = new Color(0.03f, 0.035f, 0.05f, 1f);
        icon = CreateImage(slotObject.transform, "Icon", new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(36f, 36f));
        icon.preserveAspect = true;
        label = CreateText(slotObject.transform, "Label", "空槽", 18, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(62f, 0f), new Vector2(250f, 42f));
        label.color = new Color(0.9f, 0.93f, 0.98f, 1f);
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

    private static Image CreateImage(Transform parent, string name, Vector2 anchor, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject imageObject = new GameObject(name);
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.AddComponent<Image>();
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return image;
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

    private static void SetFloatField(Object target, string fieldName, float value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(fieldName);
        property.floatValue = value;
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
