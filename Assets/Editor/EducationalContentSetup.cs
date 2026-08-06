using System.IO;
using Combat;
using Enemy;
using Player;
using UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
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

        [MenuItem("NTHUGDC/Setup Educational Content")]
        public static void Setup()
        {
            EnsureFolders();

            var projectile = CreateProjectilePrefab($"{Root}/Prefabs/Projectile_Bullet.prefab");
            var bulletData = CreateProjectileData($"{Root}/Data/ProjectileData_Bullet.asset", projectile, 14f, 3f);

            var pistolData = CreateWeaponData($"{Root}/Data/WeaponData_Pistol.asset", "Pistol", 1, 0.3f, 0.1f, 3, 1.2f);
            var axeData = CreateWeaponData($"{Root}/Data/WeaponData_Axe.asset", "Axe", 2, 0.7f, 0.18f, 3, 1.5f);
            var enemyData = CreateEnemyData($"{Root}/Data/EnemyData_Basic.asset", 3, 2.5f, 5f);

            var ranged = CreateRangedWeaponPrefab($"{Root}/Prefabs/WeaponTemplate_Ranged.prefab", pistolData, bulletData);
            var melee = CreateMeleeWeaponPrefab($"{Root}/Prefabs/WeaponTemplate_Melee.prefab", axeData);
            var segment = CreateUiSegmentPrefab($"{Root}/Prefabs/UISegment.prefab");
            var uis = EnsureUisPrefab($"{Root}/Prefabs/UIs.prefab", segment);
            CreatePlayerPrefab($"{Root}/Prefabs/Player.prefab", ranged);
            CreateEnemyPrefab($"{Root}/Prefabs/EnemyTemplate.prefab", enemyData, melee);
            EnsureCharacterHud($"{Root}/Prefabs/Player.prefab", uis);
            EnsureCharacterHud($"{Root}/Prefabs/EnemyTemplate.prefab", uis);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[NTHUGDC] Educational content created under Assets/EducationalContent. Assign Kenney sprites in Prefabs as needed.");
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

        private static void CreatePlayerPrefab(string path, GameObject rangedWeaponPrefab)
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
            root.AddComponent<PlayerAim>();
            root.AddComponent<PlayerWeaponHolder>();
            root.AddComponent<PlayerAttacker>();
            root.AddComponent<PlayerPickup>();

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
