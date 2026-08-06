using System.IO;
using Combat;
using Enemy;
using Game;
using Player;
using TMPro;
using UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Visuals;

namespace Educational.Editor
{
    /// <summary>
    /// Builds sample ScriptableObjects and Prefab templates for the OOP lab.
    /// Menu: NTHUGDC / Setup Educational Content
    /// </summary>
    public static class EducationalContentSetup
    {
        private const string Root = "Assets/EducationalContent";
        private const string AutoSetupSessionKey = "NTHUGDC_AutoGameMenusSetup_v2";

        [InitializeOnLoadMethod]
        private static void EnsurePlayerDataOnLoad()
        {
            EditorApplication.delayCall += TryPatchPlayerData;
        }

        private static void TryPatchPlayerData()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += TryPatchPlayerData;
                return;
            }

            const string playerPath = Root + "/Prefabs/Player.prefab";
            const string dataPath = Root + "/Data/PlayerData_Default.asset";
            if (!File.Exists(playerPath) || !File.Exists(dataPath))
                return;

            var player = AssetDatabase.LoadAssetAtPath<GameObject>(playerPath);
            var data = AssetDatabase.LoadAssetAtPath<PlayerData>(dataPath);
            if (player == null || data == null)
                return;

            var controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                var so = new SerializedObject(controller);
                if (so.FindProperty("data").objectReferenceValue != null)
                    return;
            }

            EnsurePlayerData(playerPath, data);
            AssetDatabase.SaveAssets();
            Debug.Log("[NTHUGDC] Patched PlayerData onto Player prefab.");
        }

        [InitializeOnLoadMethod]
        private static void EnsureEnemyCountHudOnLoad()
        {
            EditorApplication.delayCall += TryPatchEnemyCountHud;
        }

        private static void TryPatchEnemyCountHud()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += TryPatchEnemyCountHud;
                return;
            }

            const string menusPath = Root + "/Prefabs/GameMenus.prefab";
            if (!File.Exists(menusPath))
                return;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(menusPath);
            if (prefab == null || prefab.GetComponentInChildren<EnemyCountDisplay>(true) != null)
                return;

            PatchGameMenusPrefab(menusPath);
            AssetDatabase.SaveAssets();
            Debug.Log("[NTHUGDC] Patched EnemyCountHUD onto GameMenus prefab.");
        }

        [InitializeOnLoadMethod]
        private static void AutoWireGameMenusAfterCompile()
        {
            if (SessionState.GetBool(AutoSetupSessionKey, false))
                return;

            EditorApplication.delayCall += () =>
            {
                if (SessionState.GetBool(AutoSetupSessionKey, false))
                    return;
                if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                {
                    EditorApplication.delayCall += AutoWireGameMenusAfterCompileDeferred;
                    return;
                }

                AutoWireGameMenusAfterCompileDeferred();
            };
        }

        private static void AutoWireGameMenusAfterCompileDeferred()
        {
            if (SessionState.GetBool(AutoSetupSessionKey, false))
                return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += AutoWireGameMenusAfterCompileDeferred;
                return;
            }

            try
            {
                EnsureFolders();
                string menusPath = $"{Root}/Prefabs/GameMenus.prefab";
                bool needPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(menusPath) == null;
                bool needScene = !SceneHasGameManager();

                if (AssetDatabase.LoadAssetAtPath<GameObject>(menusPath) != null)
                    PatchGameMenusPrefab(menusPath);

                if (needPrefab || needScene)
                {
                    EnsureGameMenusPrefab(menusPath);
                    EnsureBaseSceneGameMenus(menusPath);
                    AssetDatabase.SaveAssets();
                    Debug.Log("[NTHUGDC] Auto-wired GameMenus / EventSystem into BaseScene.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[NTHUGDC] Auto GameMenus setup deferred: {ex.Message}");
                return;
            }

            SessionState.SetBool(AutoSetupSessionKey, true);
        }

        private static bool SceneHasGameManager()
        {
            const string scenePath = "Assets/Scenes/BaseScene.unity";
            if (!File.Exists(scenePath))
                return true;

            // Avoid opening the scene if the user has unsaved work elsewhere:
            // prefer a lightweight YAML scan for GameManager MonoBehaviour.
            string yaml = File.ReadAllText(scenePath);
            if (yaml.Contains("GameManager") || yaml.Contains("GameMenus"))
                return true;

            // Script GUID check (Assets/Scripts/Game/GameManager.cs.meta)
            return yaml.Contains("a1b2c3d4e5f60718293a4b5c6d7e8f90");
        }

        [MenuItem("NTHUGDC/Setup Educational Content")]
        public static void Setup()
        {
            EnsureFolders();

            var projectile = CreateProjectilePrefab($"{Root}/Prefabs/Projectile_Bullet.prefab");
            var bulletData = CreateProjectileData($"{Root}/Data/ProjectileData_Bullet.asset", projectile, 14f, 3f);

            var pistolData = CreateWeaponData($"{Root}/Data/WeaponData_Pistol.asset", "Pistol", 1, 0.3f, 0.1f, 3, 1.2f);
            var axeData = CreateWeaponData($"{Root}/Data/WeaponData_Axe.asset", "Axe", 2, 0.7f, 0.18f, 3, 1.5f);
            var enemyData = CreateEnemyData($"{Root}/Data/EnemyData_Basic.asset", 3, 2.5f, 5f);
            var playerData = CreatePlayerData($"{Root}/Data/PlayerData_Default.asset", 5, 5f);

            var ranged = CreateRangedWeaponPrefab($"{Root}/Prefabs/WeaponTemplate_Ranged.prefab", pistolData, bulletData);
            var melee = CreateMeleeWeaponPrefab($"{Root}/Prefabs/WeaponTemplate_Melee.prefab", axeData);
            var segment = CreateUiSegmentPrefab($"{Root}/Prefabs/UISegment.prefab");
            var uis = EnsureUisPrefab($"{Root}/Prefabs/UIs.prefab", segment);
            CreatePlayerPrefab($"{Root}/Prefabs/Player.prefab", ranged, playerData);
            CreateEnemyPrefab($"{Root}/Prefabs/EnemyTemplate.prefab", enemyData, melee);
            EnsurePlayerData($"{Root}/Prefabs/Player.prefab", playerData);
            EnsureCharacterHud($"{Root}/Prefabs/Player.prefab", uis);
            EnsureCharacterHud($"{Root}/Prefabs/EnemyTemplate.prefab", uis);
            EnsureGameMenusPrefab($"{Root}/Prefabs/GameMenus.prefab");
            EnsureBaseSceneGameMenus($"{Root}/Prefabs/GameMenus.prefab");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[NTHUGDC] Educational content created under Assets/EducationalContent. Assign Kenney sprites in Prefabs as needed.");
        }

        [MenuItem("NTHUGDC/Setup Game Menus In BaseScene")]
        public static void SetupGameMenusOnly()
        {
            EnsureFolders();
            EnsureGameMenusPrefab($"{Root}/Prefabs/GameMenus.prefab");
            EnsureBaseSceneGameMenus($"{Root}/Prefabs/GameMenus.prefab");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[NTHUGDC] GameMenus prefab + BaseScene wiring done.");
        }

        private static void EnsureFolders()
        {
            CreateFolder("Assets", "EducationalContent");
            CreateFolder(Root, "Data");
            CreateFolder(Root, "Prefabs");
        }

        private static void CreateFolder(string parent, string name)
        {
            string path = $"{parent}/{name}";
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, name);
        }

        private static ProjectileData CreateProjectileData(string path, GameObject prefab, float speed, float lifetime)
        {
            var existing = AssetDatabase.LoadAssetAtPath<ProjectileData>(path);
            if (existing != null)
            {
                if (existing.prefab == null && prefab != null)
                {
                    existing.prefab = prefab;
                    EditorUtility.SetDirty(existing);
                }
                return existing;
            }

            var data = ScriptableObject.CreateInstance<ProjectileData>();
            data.prefab = prefab;
            data.speed = speed;
            data.lifetime = lifetime;
            AssetDatabase.CreateAsset(data, path);
            return data;
        }

        private static WeaponData CreateWeaponData(
            string path,
            string displayName,
            int damage,
            float cooldown,
            float hurtDuration,
            int maxAmmo,
            float reloadTimePerAmmo)
        {
            var existing = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
            if (existing != null)
            {
                // Backfill ammo fields on older assets.
                if (existing.maxAmmo < 1)
                    existing.maxAmmo = maxAmmo;
                if (existing.reloadTimePerAmmo < 0.05f)
                    existing.reloadTimePerAmmo = reloadTimePerAmmo;
                EditorUtility.SetDirty(existing);
                return existing;
            }

            var data = ScriptableObject.CreateInstance<WeaponData>();
            data.displayName = displayName;
            data.damage = damage;
            data.attackCooldown = cooldown;
            data.hurtboxActiveDuration = hurtDuration;
            data.maxAmmo = maxAmmo;
            data.reloadTimePerAmmo = reloadTimePerAmmo;
            AssetDatabase.CreateAsset(data, path);
            return data;
        }

        private static EnemyData CreateEnemyData(string path, int hp, float speed, float aggro)
        {
            var existing = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (existing != null)
                return existing;

            var data = ScriptableObject.CreateInstance<EnemyData>();
            data.maxHealth = hp;
            data.moveSpeed = speed;
            data.aggroRange = aggro;
            AssetDatabase.CreateAsset(data, path);
            return data;
        }

        private static PlayerData CreatePlayerData(string path, int hp, float speed)
        {
            var existing = AssetDatabase.LoadAssetAtPath<PlayerData>(path);
            if (existing != null)
                return existing;

            var data = ScriptableObject.CreateInstance<PlayerData>();
            data.maxHealth = hp;
            data.moveSpeed = speed;
            AssetDatabase.CreateAsset(data, path);
            return data;
        }

        private static GameObject CreateProjectilePrefab(string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
                return existing;

            var root = new GameObject("Projectile_Bullet");
            var rb = root.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = root.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.12f;

            root.AddComponent<Damager>();
            root.AddComponent<ProjectileMover>();
            root.AddComponent<ProjectileHit>();

            var sr = root.AddComponent<SpriteRenderer>();
            sr.sprite = GetDefaultSprite();
            sr.color = Color.yellow;

            root.tag = "Projectile";

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateRangedWeaponPrefab(string path, WeaponData data, ProjectileData projectileData)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                var existingWeapon = existing.GetComponent<RangedWeapon>();
                if (existingWeapon != null && projectileData != null)
                {
                    var existingSo = new SerializedObject(existingWeapon);
                    var projProp = existingSo.FindProperty("projectileData");
                    if (projProp != null && projProp.objectReferenceValue == null)
                    {
                        projProp.objectReferenceValue = projectileData;
                        existingSo.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(existing);
                    }
                }
                return existing;
            }

            var root = new GameObject("WeaponTemplate_Ranged");
            root.AddComponent<WeaponOwner>();
            var weapon = root.AddComponent<RangedWeapon>();
            root.AddComponent<WeaponPickup>();
            var pickupCol = root.AddComponent<CircleCollider2D>();
            pickupCol.isTrigger = true;
            pickupCol.radius = 0.4f;

            var sprite = new GameObject("Sprite");
            sprite.transform.SetParent(root.transform, false);
            var sr = sprite.AddComponent<SpriteRenderer>();
            sr.sprite = GetDefaultSprite();
            sr.color = new Color(0.3f, 0.7f, 1f);

            var hurt = new GameObject("HurtBox");
            hurt.transform.SetParent(root.transform, false);
            hurt.SetActive(false);
            var hurtCol = hurt.AddComponent<BoxCollider2D>();
            hurtCol.isTrigger = true;
            hurtCol.size = new Vector2(0.4f, 0.2f);
            hurt.AddComponent<Damager>();
            hurt.AddComponent<HurtBox>();

            var so = new SerializedObject(weapon);
            so.FindProperty("data").objectReferenceValue = data;
            so.FindProperty("projectileData").objectReferenceValue = projectileData;
            so.FindProperty("hurtBoxObject").objectReferenceValue = hurt;
            var ownerProp = so.FindProperty("owner");
            if (ownerProp != null)
                ownerProp.objectReferenceValue = root.GetComponent<WeaponOwner>();
            so.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateMeleeWeaponPrefab(string path, WeaponData data)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
                return existing;

            var root = new GameObject("WeaponTemplate_Melee");
            root.AddComponent<WeaponOwner>();
            var weapon = root.AddComponent<MeleeWeapon>();
            root.AddComponent<WeaponPickup>();
            var pickupCol = root.AddComponent<CircleCollider2D>();
            pickupCol.isTrigger = true;
            pickupCol.radius = 0.4f;

            var sprite = new GameObject("Sprite");
            sprite.transform.SetParent(root.transform, false);
            var sr = sprite.AddComponent<SpriteRenderer>();
            sr.sprite = GetDefaultSprite();
            sr.color = new Color(1f, 0.5f, 0.2f);
            sprite.AddComponent<MeleeSwingVisual>();

            var hurt = new GameObject("HurtBox");
            hurt.transform.SetParent(root.transform, false);
            hurt.SetActive(false);
            var hurtCol = hurt.AddComponent<BoxCollider2D>();
            hurtCol.isTrigger = true;
            hurtCol.size = new Vector2(0.8f, 0.5f);
            hurt.AddComponent<Damager>();
            hurt.AddComponent<HurtBox>();

            var so = new SerializedObject(weapon);
            so.FindProperty("data").objectReferenceValue = data;
            so.FindProperty("hurtBoxObject").objectReferenceValue = hurt;
            var ownerProp = so.FindProperty("owner");
            if (ownerProp != null)
                ownerProp.objectReferenceValue = root.GetComponent<WeaponOwner>();
            so.FindProperty("hurtBox").objectReferenceValue = hurt.GetComponent<HurtBox>();
            so.FindProperty("swingVisual").objectReferenceValue = sprite.GetComponent<MeleeSwingVisual>();
            so.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateUiSegmentPrefab(string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                // Patch world-space sizing / sliced frame on older segment prefabs.
                string existingPath = AssetDatabase.GetAssetPath(existing);
                GameObject contents = PrefabUtility.LoadPrefabContents(existingPath);
                try
                {
                    var rt = contents.GetComponent<RectTransform>();
                    if (rt != null)
                        rt.sizeDelta = new Vector2(0.2f, 0.2f);

                    var frame = contents.GetComponent<Image>();
                    if (frame != null)
                    {
                        frame.sprite = GetUiFrameSprite();
                        frame.type = Image.Type.Sliced;
                        frame.pixelsPerUnitMultiplier = 8f;
                    }

                    PrefabUtility.SaveAsPrefabAsset(contents, existingPath);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }

                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            var root = new GameObject("UISegment", typeof(RectTransform));
            var rootRt = root.GetComponent<RectTransform>();
            rootRt.sizeDelta = new Vector2(0.2f, 0.2f);

            var frameImg = root.AddComponent<Image>();
            frameImg.sprite = GetUiFrameSprite();
            frameImg.type = Image.Type.Sliced;
            frameImg.pixelsPerUnitMultiplier = 8f;
            frameImg.raycastTarget = false;
            frameImg.color = Color.white;

            var fillGo = new GameObject("Fill", typeof(RectTransform));
            fillGo.transform.SetParent(root.transform, false);
            var fillRt = fillGo.GetComponent<RectTransform>();
            fillRt.anchorMin = new Vector2(0.15f, 0.15f);
            fillRt.anchorMax = new Vector2(0.85f, 0.85f);
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;

            var fill = fillGo.AddComponent<Image>();
            fill.sprite = GetUiFillSprite();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 1f;
            fill.raycastTarget = false;
            fill.color = new Color(0.25f, 0.85f, 0.35f, 1f);

            var view = root.AddComponent<SegmentView>();
            var viewSo = new SerializedObject(view);
            viewSo.FindProperty("frame").objectReferenceValue = frameImg;
            viewSo.FindProperty("fill").objectReferenceValue = fill;
            viewSo.ApplyModifiedPropertiesWithoutUndo();

            // Template stays inactive so layout only shows runtime instances.
            root.SetActive(false);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject EnsureUisPrefab(string path, GameObject segmentPrefab)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                // Keep segment prefab refs wired on existing bars.
                string existingPath = AssetDatabase.GetAssetPath(existing);
                GameObject contents = PrefabUtility.LoadPrefabContents(existingPath);
                try
                {
                    SegmentView segmentView = segmentPrefab != null
                        ? segmentPrefab.GetComponent<SegmentView>()
                        : null;
                    foreach (var bar in contents.GetComponentsInChildren<SegmentedBar>(true))
                    {
                        var so = new SerializedObject(bar);
                        if (segmentView != null)
                            so.FindProperty("segmentPrefab").objectReferenceValue = segmentView;
                        so.FindProperty("container").objectReferenceValue = bar.transform;
                        so.ApplyModifiedPropertiesWithoutUndo();
                    }

                    PrefabUtility.SaveAsPrefabAsset(contents, existingPath);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }

                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            var root = new GameObject("UIs", typeof(RectTransform));
            var rootRt = root.GetComponent<RectTransform>();
            rootRt.sizeDelta = new Vector2(2.1f, 0.5f);
            rootRt.anchoredPosition = new Vector2(0f, 1f);

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 20;

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 16f;
            scaler.referencePixelsPerUnit = 100f;
            root.AddComponent<GraphicRaycaster>();

            SegmentView segmentViewNew = segmentPrefab != null
                ? segmentPrefab.GetComponent<SegmentView>()
                : null;

            CreateBarOnUis(root.transform, "Health", new Vector2(0f, 0.55f), new Vector2(1f, 1f),
                new Color(0.42f, 1f, 0f, 1f), segmentViewNew);
            CreateBarOnUis(root.transform, "Ammo", new Vector2(0f, 0f), new Vector2(1f, 0.45f),
                new Color(1f, 0.98f, 0f, 1f), segmentViewNew);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void CreateBarOnUis(
            Transform parent,
            string barName,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color fillTint,
            SegmentView segmentView)
        {
            var barGo = new GameObject(barName, typeof(RectTransform));
            barGo.transform.SetParent(parent, false);
            var rt = barGo.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);

            var layout = barGo.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.spacing = 0f;

            var bar = barGo.AddComponent<SegmentedBar>();
            var barSo = new SerializedObject(bar);
            barSo.FindProperty("segmentPrefab").objectReferenceValue = segmentView;
            barSo.FindProperty("container").objectReferenceValue = barGo.transform;
            barSo.FindProperty("fillColor").colorValue = fillTint;
            barSo.FindProperty("overrideFillColor").boolValue = true;
            barSo.ApplyModifiedPropertiesWithoutUndo();

            if (barName == "Health")
            {
                var display = barGo.AddComponent<HealthBarDisplay>();
                var so = new SerializedObject(display);
                so.FindProperty("bar").objectReferenceValue = bar;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                var display = barGo.AddComponent<AmmoBarDisplay>();
                var so = new SerializedObject(display);
                so.FindProperty("bar").objectReferenceValue = bar;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        /// <summary>
        /// Nests the shared UIs prefab under a character and wires Health / WeaponPivot.
        /// </summary>
        private static void EnsureCharacterHud(string characterPath, GameObject uisPrefab)
        {
            var character = AssetDatabase.LoadAssetAtPath<GameObject>(characterPath);
            if (character == null || uisPrefab == null)
                return;

            string assetPath = AssetDatabase.GetAssetPath(character);
            GameObject root = PrefabUtility.LoadPrefabContents(assetPath);

            try
            {
                // Remove legacy embedded Canvas (pre-UIs split).
                Transform legacyCanvas = root.transform.Find("Canvas");
                if (legacyCanvas != null)
                    Object.DestroyImmediate(legacyCanvas.gameObject);

                Transform uisTf = root.transform.Find("UIs");
                GameObject uisInstance;
                if (uisTf == null)
                {
                    uisInstance = (GameObject)PrefabUtility.InstantiatePrefab(uisPrefab, root.transform);
                    uisInstance.name = "UIs";
                    uisInstance.transform.SetAsFirstSibling();
                    var rt = uisInstance.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchoredPosition = new Vector2(0f, 1f);
                        rt.localScale = Vector3.one;
                    }
                }
                else
                {
                    uisInstance = uisTf.gameObject;
                }

                Health health = root.GetComponent<Health>();
                WeaponPivot pivot = root.GetComponentInChildren<WeaponPivot>(true);

                var healthDisplay = uisInstance.GetComponentInChildren<HealthBarDisplay>(true);
                if (healthDisplay != null)
                {
                    var so = new SerializedObject(healthDisplay);
                    so.FindProperty("health").objectReferenceValue = health;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                var ammoDisplay = uisInstance.GetComponentInChildren<AmmoBarDisplay>(true);
                if (ammoDisplay != null)
                {
                    var so = new SerializedObject(ammoDisplay);
                    so.FindProperty("weaponPivot").objectReferenceValue = pivot;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                PrefabUtility.SaveAsPrefabAsset(root, assetPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void CreatePlayerPrefab(string path, GameObject rangedWeaponPrefab, PlayerData data)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                return;

            var root = new GameObject("Player");
            root.tag = "Player";

            var rb = root.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            var bodyCol = root.AddComponent<CircleCollider2D>();
            bodyCol.radius = 0.35f;

            root.AddComponent<Health>();
            root.AddComponent<PlayerMover>();
            var controller = root.AddComponent<PlayerController>();
            root.AddComponent<PlayerAim>();
            root.AddComponent<PlayerWeaponHolder>();
            root.AddComponent<PlayerAttacker>();
            root.AddComponent<PlayerPickup>();

            var controllerSo = new SerializedObject(controller);
            controllerSo.FindProperty("data").objectReferenceValue = data;
            controllerSo.ApplyModifiedPropertiesWithoutUndo();

            var playerInput = root.AddComponent<PlayerInput>();
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            if (actions != null)
            {
                playerInput.actions = actions;
                playerInput.defaultActionMap = "Player";
                playerInput.notificationBehavior = PlayerNotifications.SendMessages;
            }

            var sprite = new GameObject("Sprite");
            sprite.transform.SetParent(root.transform, false);
            var sr = sprite.AddComponent<SpriteRenderer>();
            sr.sprite = GetDefaultSprite();
            sr.color = Color.white;
            var waddle = sprite.AddComponent<VelocityWaddleTween>();
            var waddleSo = new SerializedObject(waddle);
            waddleSo.FindProperty("body").objectReferenceValue = rb;
            waddleSo.ApplyModifiedPropertiesWithoutUndo();

            var pivotGo = new GameObject("WeaponPivot");
            pivotGo.transform.SetParent(root.transform, false);
            var pivot = pivotGo.AddComponent<WeaponPivot>();

            var holder = root.GetComponent<PlayerWeaponHolder>();
            var holderSo = new SerializedObject(holder);
            holderSo.FindProperty("weaponPivot").objectReferenceValue = pivot;
            holderSo.ApplyModifiedPropertiesWithoutUndo();

            if (rangedWeaponPrefab != null)
            {
                var weaponInstance = (GameObject)PrefabUtility.InstantiatePrefab(rangedWeaponPrefab);
                weaponInstance.name = "Weapon";
                weaponInstance.transform.SetParent(pivotGo.transform, false);
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void EnsurePlayerData(string characterPath, PlayerData data)
        {
            var character = AssetDatabase.LoadAssetAtPath<GameObject>(characterPath);
            if (character == null || data == null)
                return;

            string assetPath = AssetDatabase.GetAssetPath(character);
            GameObject root = PrefabUtility.LoadPrefabContents(assetPath);

            try
            {
                var controller = root.GetComponent<PlayerController>();
                if (controller == null)
                    controller = root.AddComponent<PlayerController>();

                var so = new SerializedObject(controller);
                so.FindProperty("data").objectReferenceValue = data;
                so.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, assetPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void CreateEnemyPrefab(string path, EnemyData data, GameObject meleeWeaponPrefab)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                return;

            var root = new GameObject("EnemyTemplate");
            root.tag = "Enemy";

            var rb = root.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            var bodyCol = root.AddComponent<CircleCollider2D>();
            bodyCol.radius = 0.35f;

            root.AddComponent<Health>();
            root.AddComponent<EnemyMovement>();
            var controller = root.AddComponent<EnemyController>();

            var sprite = new GameObject("Sprite");
            sprite.transform.SetParent(root.transform, false);
            var sr = sprite.AddComponent<SpriteRenderer>();
            sr.sprite = GetDefaultSprite();
            sr.color = new Color(1f, 0.3f, 0.3f);
            var waddle = sprite.AddComponent<VelocityWaddleTween>();
            var waddleSo = new SerializedObject(waddle);
            waddleSo.FindProperty("body").objectReferenceValue = rb;
            waddleSo.ApplyModifiedPropertiesWithoutUndo();

            var aggro = new GameObject("AggroZone");
            aggro.transform.SetParent(root.transform, false);
            var aggroCol = aggro.AddComponent<CircleCollider2D>();
            aggroCol.isTrigger = true;
            aggroCol.radius = data != null ? data.aggroRange : 5f;
            aggro.AddComponent<AggroZone>();

            var pivotGo = new GameObject("WeaponPivot");
            pivotGo.transform.SetParent(root.transform, false);
            var pivot = pivotGo.AddComponent<WeaponPivot>();

            if (meleeWeaponPrefab != null)
            {
                var weaponInstance = (GameObject)PrefabUtility.InstantiatePrefab(meleeWeaponPrefab);
                weaponInstance.name = "Weapon";
                weaponInstance.transform.SetParent(pivotGo.transform, false);
            }

            var so = new SerializedObject(controller);
            so.FindProperty("data").objectReferenceValue = data;
            so.FindProperty("aggroZone").objectReferenceValue = aggro.GetComponent<AggroZone>();
            so.FindProperty("weaponPivot").objectReferenceValue = pivot;
            so.FindProperty("weapon").objectReferenceValue = root.GetComponentInChildren<Weapon>();
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static GameObject EnsureGameMenusPrefab(string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                PatchGameMenusPrefab(path);
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

            var root = new GameObject("GameMenus", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();

            var manager = root.AddComponent<GameManager>();
            CreateEnemyCountHud(root.transform, font);

            // Pause panel — instant show/hide
            GameObject pausePanel = CreateMenuPanel(root.transform, "PausePanel", new Color(0f, 0f, 0f, 0.65f));
            CreateTmpLabel(pausePanel.transform, "Title", "Paused", 72f, font, new Vector2(0f, 80f));
            Button pauseRestart = CreateMenuButton(pausePanel.transform, "RestartButton", "Restart", font, new Vector2(0f, -40f));
            pausePanel.SetActive(false);

            // Game over panel — CanvasGroup fade
            GameObject gameOverPanel = CreateMenuPanel(root.transform, "GameOverPanel", new Color(0f, 0f, 0f, 0.75f));
            var canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            TMP_Text gameOverTitle = CreateTmpLabel(gameOverPanel.transform, "Title", "You Win!", 72f, font, new Vector2(0f, 80f));
            Button gameOverRestart = CreateMenuButton(gameOverPanel.transform, "RestartButton", "Restart", font, new Vector2(0f, -40f));
            gameOverPanel.SetActive(false);

            var so = new SerializedObject(manager);
            so.FindProperty("pausePanel").objectReferenceValue = pausePanel;
            so.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
            so.FindProperty("gameOverCanvasGroup").objectReferenceValue = canvasGroup;
            so.FindProperty("gameOverTitle").objectReferenceValue = gameOverTitle;
            so.FindProperty("pauseRestartButton").objectReferenceValue = pauseRestart;
            so.FindProperty("gameOverRestartButton").objectReferenceValue = gameOverRestart;
            so.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void PatchGameMenusPrefab(string path)
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (contents.GetComponentInChildren<EnemyCountDisplay>(true) != null)
                    return;

                TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                    "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
                CreateEnemyCountHud(contents.transform, font);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static void CreateEnemyCountHud(Transform parent, TMP_FontAsset font)
        {
            var hud = new GameObject("EnemyCountHUD", typeof(RectTransform));
            hud.transform.SetParent(parent, false);
            var rt = hud.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(24f, -24f);
            rt.sizeDelta = new Vector2(320f, 48f);

            var bg = hud.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.45f);
            bg.raycastTarget = false;

            var layout = hud.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.spacing = 8f;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(12, 12, 6, 6);

            TMP_Text label = CreateHudCountText(hud.transform, "Label", "Enemies", 28f, font, 110f);
            TMP_Text remaining = CreateHudCountText(hud.transform, "Remaining", "0", 32f, font, 36f);
            TMP_Text slash = CreateHudCountText(hud.transform, "Slash", "/", 28f, font, 16f);
            TMP_Text total = CreateHudCountText(hud.transform, "Total", "0", 32f, font, 36f);

            var display = hud.AddComponent<EnemyCountDisplay>();
            var so = new SerializedObject(display);
            so.FindProperty("labelText").objectReferenceValue = label;
            so.FindProperty("remainingText").objectReferenceValue = remaining;
            so.FindProperty("slashText").objectReferenceValue = slash;
            so.FindProperty("totalText").objectReferenceValue = total;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static TMP_Text CreateHudCountText(
            Transform parent,
            string name,
            string text,
            float fontSize,
            TMP_FontAsset font,
            float width)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(width, 36f);

            var layoutElement = go.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = width;
            layoutElement.minWidth = width;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            if (font != null)
                tmp.font = font;
            return tmp;
        }

        private static void EnsureBaseSceneGameMenus(string menusPrefabPath)
        {
            const string scenePath = "Assets/Scenes/BaseScene.unity";
            if (!File.Exists(scenePath))
            {
                Debug.LogWarning($"[NTHUGDC] Scene missing: {scenePath}");
                return;
            }

            var menusPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(menusPrefabPath);
            if (menusPrefab == null)
            {
                Debug.LogWarning($"[NTHUGDC] GameMenus prefab missing: {menusPrefabPath}");
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // EventSystem (Input System UI module)
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<InputSystemUIInputModule>();
                Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
            }

            GameManager existingManager = Object.FindFirstObjectByType<GameManager>();
            if (existingManager == null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(menusPrefab);
                instance.name = "GameMenus";
                Undo.RegisterCreatedObjectUndo(instance, "Create GameMenus");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static GameObject CreateMenuPanel(Transform parent, string name, Color dimColor)
        {
            var panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = panel.AddComponent<Image>();
            img.color = dimColor;
            img.raycastTarget = true;
            return panel;
        }

        private static TMP_Text CreateTmpLabel(
            Transform parent,
            string name,
            string text,
            float fontSize,
            TMP_FontAsset font,
            Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(800f, 120f);
            rt.anchoredPosition = anchoredPos;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            if (font != null)
                tmp.font = font;
            return tmp;
        }

        private static Button CreateMenuButton(
            Transform parent,
            string name,
            string label,
            TMP_FontAsset font,
            Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(280f, 64f);
            rt.anchoredPosition = anchoredPos;

            var img = go.AddComponent<Image>();
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;
            img.color = new Color(0.2f, 0.55f, 0.95f, 1f);

            var button = go.AddComponent<Button>();
            button.targetGraphic = img;

            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(go.transform, false);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 36f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            if (font != null)
                tmp.font = font;

            return button;
        }

        private static Sprite GetDefaultSprite()
        {
            // Built-in knob is available in editor resources.
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        }

        private static Sprite GetUiFrameSprite()
        {
            const string path = "Assets/kenney_desert-shooter-pack_1.0/PNG/Interface/Tilemap/tilemap_packed.png";
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Sprite sprite && sprite.name == "tilemap_packed_80")
                    return sprite;
            }

            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        }

        private static Sprite GetUiFillSprite()
        {
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd")
                   ?? AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        }
    }
}
