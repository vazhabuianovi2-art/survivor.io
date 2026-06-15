using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.IO;

/// <summary>
/// One-shot editor script that:
/// 1. Creates enemy prefabs (Goblin, Skeleton, Bat, MiniBoss, Boss) from existing asset-pack prefabs
/// 2. Creates a lava/dungeon background material and quad
/// 3. Adds WaveManager to scene, wires all prefabs
/// 4. Adds WinUI, BossHealthBar to scene canvas
/// </summary>
public class SetupFullGame
{
    const string XP_GEM = "Assets/Prefabs/XPGem.prefab";

    public static string Execute()
    {
        var log = new System.Text.StringBuilder();

        // ── 1. Load source prefabs from asset packs ───────────────────────
        var goblinSrc   = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Dark fantasy - popular enemies- Free Sample/Goblin/Goblin_Thief.prefab");
        var skelSrc     = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Dark fantasy - popular enemies- Free Sample/Skeleton/skeleton.prefab");
        var batSrc      = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ArtApex Studio/Prefabs/Bat_01.prefab");
        var darkKnight  = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Dark Knight/Prefabs/Dark Knight.prefab");
        var xpGem       = AssetDatabase.LoadAssetAtPath<GameObject>(XP_GEM);

        log.AppendLine($"goblinSrc={goblinSrc?.name} skelSrc={skelSrc?.name} batSrc={batSrc?.name} darkKnight={darkKnight?.name}");

        if (!Directory.Exists("Assets/Prefabs/Enemies"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "Enemies");

        // ── Helper: build a prefab from a visual source + Enemy component ──
        GameObject MakeEnemyPrefab(string path, GameObject visual, float hp, float spd, float dmg, float scale, int gems, Color? tint = null)
        {
            var go = new GameObject(Path.GetFileNameWithoutExtension(path));

            // Rigidbody + Collider
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.22f * scale;

            go.tag = "Enemy";
            go.layer = LayerMask.NameToLayer("Default");

            // Health
            var health = go.AddComponent<SurvivorIO.Health>();
            var soH = new SerializedObject(health);
            soH.FindProperty("maxHealth").floatValue = hp;
            soH.ApplyModifiedPropertiesWithoutUndo();

            // Enemy script
            var enemy = go.AddComponent<SurvivorIO.Enemy>();
            var soE = new SerializedObject(enemy);
            soE.FindProperty("moveSpeed").floatValue = spd;
            soE.FindProperty("contactDamage").floatValue = dmg;
            soE.FindProperty("xpGemCount").intValue = gems;
            if (xpGem != null) soE.FindProperty("xpGemPrefab").objectReferenceValue = xpGem;
            soE.ApplyModifiedPropertiesWithoutUndo();

            // Visual child
            if (visual != null)
            {
                var vis = (GameObject)PrefabUtility.InstantiatePrefab(visual, go.transform);
                vis.transform.localPosition = Vector3.zero;
                vis.transform.localScale = Vector3.one * scale;
                if (tint.HasValue)
                {
                    foreach (var sr in vis.GetComponentsInChildren<SpriteRenderer>())
                        sr.color = tint.Value;
                }
                // Wire spriteToFlip on Enemy
                var sr0 = vis.GetComponentInChildren<SpriteRenderer>();
                if (sr0 != null)
                {
                    soE = new SerializedObject(enemy);
                    soE.FindProperty("spriteToFlip").objectReferenceValue = sr0;
                    soE.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            else
            {
                // Fallback: colored square
                var visGo = new GameObject("Visual");
                visGo.transform.SetParent(go.transform, false);
                var sr = visGo.AddComponent<SpriteRenderer>();
                sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                sr.color = tint ?? Color.white;
                sr.transform.localScale = Vector3.one * scale;
                soE = new SerializedObject(enemy);
                soE.FindProperty("spriteToFlip").objectReferenceValue = sr;
                soE.ApplyModifiedPropertiesWithoutUndo();
            }

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            log.AppendLine($"Created prefab: {path}");
            return prefab;
        }

        // ── 2. Create enemy prefabs ───────────────────────────────────────
        var goblinPrefab   = MakeEnemyPrefab("Assets/Prefabs/Enemies/Goblin.prefab",     goblinSrc,  40f,  2.5f,  8f, 0.6f, 1);
        var orcPrefab      = MakeEnemyPrefab("Assets/Prefabs/Enemies/Orc.prefab",        goblinSrc, 80f,  2.0f, 12f, 0.8f, 2, new Color(0.4f, 0.8f, 0.3f));
        var skelPrefab     = MakeEnemyPrefab("Assets/Prefabs/Enemies/Skeleton.prefab",   skelSrc,   60f,  2.2f, 10f, 0.6f, 2);
        var batPrefab      = MakeEnemyPrefab("Assets/Prefabs/Enemies/Bat.prefab",        batSrc,    30f,  3.5f,  6f, 0.5f, 1);
        var miniBossPrefab = MakeEnemyPrefab("Assets/Prefabs/Enemies/MiniBoss.prefab",   goblinSrc, 500f, 1.8f, 20f, 1.3f, 5, new Color(1f, 0.5f, 0.1f));
        var bossPrefab     = MakeEnemyPrefab("Assets/Prefabs/Enemies/Boss.prefab",       darkKnight,2000f,1.2f, 35f, 2.0f,15, new Color(0.8f, 0.2f, 0.2f));

        AssetDatabase.SaveAssets();

        // ── 3. Lava background ────────────────────────────────────────────
        SetupBackground(log);

        // ── 4. WaveManager in scene ───────────────────────────────────────
        var waveManagerGo = GameObject.Find("WaveManager") ?? new GameObject("WaveManager");
        // Remove old EnemySpawner (WaveManager replaces it)
        var oldSpawner = Object.FindFirstObjectByType<SurvivorIO.EnemySpawner>();
        if (oldSpawner != null)
        {
            oldSpawner.enabled = false;
            log.AppendLine("Disabled old EnemySpawner");
        }

        var wm = waveManagerGo.GetComponent<SurvivorIO.WaveManager>();
        if (wm == null) wm = waveManagerGo.AddComponent<SurvivorIO.WaveManager>();

        var soWM = new SerializedObject(wm);

        // Set up wave configs array
        soWM.FindProperty("miniBossPrefab").objectReferenceValue = miniBossPrefab;
        soWM.FindProperty("miniBossTime").floatValue = 300f;
        soWM.FindProperty("bossPrefab").objectReferenceValue = bossPrefab;
        soWM.FindProperty("bossTime").floatValue = 900f;

        var wavesProp = soWM.FindProperty("waves");
        wavesProp.arraySize = 4;

        void SetWave(int idx, string label, GameObject pf, float unlock, float si, float mi, float ramp, int maxA)
        {
            var el = wavesProp.GetArrayElementAtIndex(idx);
            el.FindPropertyRelative("label").stringValue = label;
            el.FindPropertyRelative("prefab").objectReferenceValue = pf;
            el.FindPropertyRelative("unlockTime").floatValue = unlock;
            el.FindPropertyRelative("startInterval").floatValue = si;
            el.FindPropertyRelative("minInterval").floatValue = mi;
            el.FindPropertyRelative("rampDuration").floatValue = ramp;
            el.FindPropertyRelative("maxAlive").intValue = maxA;
        }

        SetWave(0, "Goblin",   goblinPrefab, 0f,   1.5f, 0.3f, 120f, 80);
        SetWave(1, "Orc",      orcPrefab,    180f,  2.5f, 0.6f, 120f, 40);
        SetWave(2, "Skeleton", skelPrefab,   180f,  2.0f, 0.5f, 120f, 40);
        SetWave(3, "Bat",      batPrefab,    300f,  1.8f, 0.4f,  90f, 30);

        soWM.ApplyModifiedPropertiesWithoutUndo();
        log.AppendLine("WaveManager configured");

        // ── 5. WinUI + BossHealthBar ──────────────────────────────────────
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            if (canvas.GetComponentInChildren<SurvivorIO.WinUI>() == null)
            {
                var winGo = new GameObject("WinUI");
                winGo.transform.SetParent(canvas.transform, false);
                winGo.AddComponent<SurvivorIO.WinUI>();
                log.AppendLine("Added WinUI");
            }
            if (Object.FindFirstObjectByType<SurvivorIO.BossHealthBar>() == null)
            {
                var bhbGo = new GameObject("BossHealthBar");
                bhbGo.transform.SetParent(canvas.transform, false);
                bhbGo.AddComponent<SurvivorIO.BossHealthBar>();
                log.AppendLine("Added BossHealthBar");
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return log.ToString();
    }

    static void SetupBackground(System.Text.StringBuilder log)
    {
        // Create lava material with tiling
        if (!Directory.Exists("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        const string matPath = "Assets/Materials/LavaBackground.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = new Color(0.25f, 0.08f, 0.02f); // dark lava red
            AssetDatabase.CreateAsset(mat, matPath);
            log.AppendLine("Created LavaBackground material");
        }

        // Check if background already exists
        var existing = GameObject.Find("InfiniteBackground");
        if (existing != null)
        {
            log.AppendLine("InfiniteBackground already exists");
            return;
        }

        // Create a quad as background
        var bgGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bgGo.name = "InfiniteBackground";
        Object.DestroyImmediate(bgGo.GetComponent<MeshCollider>());

        // Place behind everything
        bgGo.transform.position = new Vector3(0f, 0f, 10f);
        bgGo.transform.localScale = new Vector3(200f, 200f, 1f);

        // Assign material
        var mr = bgGo.GetComponent<MeshRenderer>();
        mr.sharedMaterial = mat;
        mr.shadowCastingMode = ShadowCastingMode.Off;
        mr.receiveShadows = false;

        // Sorting: far behind sprites
        mr.sortingOrder = -100;

        // Add InfiniteBackground script
        bgGo.AddComponent<SurvivorIO.InfiniteBackground>();

        log.AppendLine("Created InfiniteBackground quad");
    }
}
