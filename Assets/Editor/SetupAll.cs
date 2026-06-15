using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// One-shot setup: smoke FX, Slash_A, ground texture, enemy range fix.
/// </summary>
public class SetupAll
{
    public static string Execute()
    {
        var log = new System.Text.StringBuilder();

        // ── 1. Create smoke particle prefab ──────────────────────────────
        string smokePrefabPath = "Assets/Prefabs/SmokeDeathFX.prefab";
        if (!AssetDatabase.LoadAssetAtPath<GameObject>(smokePrefabPath))
        {
            var smokeRoot = new GameObject("SmokeDeathFX");
            var ps        = smokeRoot.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration          = 0.5f;
            main.loop              = false;
            main.startLifetime     = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
            main.startSpeed        = new ParticleSystem.MinMaxCurve(1.5f, 3f);
            main.startSize         = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
            main.startColor        = new ParticleSystem.MinMaxGradient(
                new Color(0.6f, 0.6f, 0.6f, 1f),
                new Color(0.85f, 0.85f, 0.85f, 0.7f));
            main.gravityModifier   = new ParticleSystem.MinMaxCurve(-0.3f, 0.1f);
            main.maxParticles      = 12;
            main.stopAction        = ParticleSystemStopAction.Destroy;
            main.simulationSpace   = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 8, 12) });

            var shape   = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius    = 0.3f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = new ParticleSystem.MinMaxGradient(grad);

            var renderer = smokeRoot.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 50;

            bool okSmoke;
            PrefabUtility.SaveAsPrefabAsset(smokeRoot, smokePrefabPath, out okSmoke);
            Object.DestroyImmediate(smokeRoot);
            log.AppendLine(okSmoke ? "Smoke prefab created ✓" : "ERROR: smoke prefab failed");
        }
        else
        {
            log.AppendLine("Smoke prefab already exists, skipping");
        }

        var smokePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(smokePrefabPath);

        // ── 2. Wire smoke + Slash_A to all enemy prefabs ─────────────────
        string slashAPrefabPath = "Assets/NamuFX/Simple Stylized Slash vol2/Prefabs/Slash_A.prefab";
        var slashAPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(slashAPrefabPath);
        if (slashAPrefab == null) log.AppendLine("WARNING: Slash_A not found at expected path");

        string[] enemyPaths = {
            "Assets/Prefabs/Enemies/Goblin.prefab",
            "Assets/Prefabs/Enemies/Orc.prefab",
            "Assets/Prefabs/Enemies/Skeleton.prefab",
            "Assets/Prefabs/Enemies/Bat.prefab",
            "Assets/Prefabs/Enemies/MiniBoss.prefab",
            "Assets/Prefabs/Enemies/Boss.prefab",
        };

        foreach (var path in enemyPaths)
        {
            var pfb = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (pfb == null) continue;

            using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                var root  = scope.prefabContentsRoot;
                var enemy = root.GetComponent<SurvivorIO.Enemy>();
                if (enemy == null) continue;

                var so = new SerializedObject(enemy);
                so.FindProperty("deathFxPrefab").objectReferenceValue = smokePrefab;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            log.AppendLine($"  Smoke wired to {System.IO.Path.GetFileNameWithoutExtension(path)}");
        }

        // ── 3. Wire Slash_A to MeleeWeapon on Player ─────────────────────
        if (slashAPrefab != null)
        {
            var player = GameObject.Find("Player");
            if (player != null)
            {
                var mw = player.GetComponent<SurvivorIO.MeleeWeapon>();
                if (mw != null)
                {
                    var so = new SerializedObject(mw);
                    so.FindProperty("slashPrefab").objectReferenceValue = slashAPrefab;
                    // Increase range to 5 so slash fires at nearby enemies, not just adjacent
                    so.FindProperty("range").floatValue = 5f;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    log.AppendLine("Slash_A wired to MeleeWeapon, range=5 ✓");
                }
            }
        }

        // ── 4. Update ground background to lava infinity.png ─────────────
        string lavaTexPath = "Assets/Reference/lava infinity.png";
        var lavaImp = AssetDatabase.LoadAssetAtPath<TextureImporter>(lavaTexPath) as TextureImporter
                   ?? (TextureImporter)AssetImporter.GetAtPath(lavaTexPath);
        if (lavaImp != null)
        {
            lavaImp.textureType      = TextureImporterType.Sprite;
            lavaImp.spriteImportMode = SpriteImportMode.Single;
            lavaImp.wrapMode         = TextureWrapMode.Repeat;
            lavaImp.filterMode       = FilterMode.Bilinear;
            lavaImp.mipmapEnabled    = false;
            lavaImp.spritePixelsPerUnit = 100f;
            EditorUtility.SetDirty(lavaImp);
            lavaImp.SaveAndReimport();
            log.AppendLine("lava infinity.png imported as tiled sprite ✓");
        }
        else
        {
            log.AppendLine("WARNING: lava infinity.png not found");
        }

        var lavaSprite = AssetDatabase.LoadAssetAtPath<Sprite>(lavaTexPath);
        var bg         = GameObject.Find("InfiniteBackground");
        if (bg != null && lavaSprite != null)
        {
            var sr = bg.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite   = lavaSprite;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.tileMode = SpriteTileMode.Continuous;
                // Keep scale as-is (large enough to fill screen)
                log.AppendLine("InfiniteBackground updated to lava infinity.png ✓");
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return log.ToString();
    }
}
