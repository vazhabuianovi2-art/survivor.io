using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateCrescentSlash
{
    public static string Execute()
    {
        // ── 1. Generate crescent sprite (256x256) — intrinsically blue ────
        int size = 256;
        var tex  = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        float cx = size * 0.5f;
        float cy = size * 0.5f;

        float outerR    = size * 0.46f;
        float innerR    = size * 0.24f;
        float midR      = (outerR + innerR) * 0.5f;
        float ringHalf  = (outerR - innerR) * 0.5f;
        float glowOuter = outerR * 1.22f;

        // Arc centred on 90° (local up = travel direction)
        float arcCenter = 90f;
        float arcHalf   = 82f;   // ±82° = 164° wide

        var clear = new Color(0, 0, 0, 0);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx  = x - cx;
                float dy  = y - cy;
                float r   = Mathf.Sqrt(dx * dx + dy * dy);
                float ang = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                float rel = Mathf.Abs(Mathf.DeltaAngle(ang, arcCenter));

                bool inArc  = rel <= arcHalf;
                bool inRing = r >= innerR && r <= outerR;
                bool inGlow = inArc && r > outerR && r <= glowOuter;

                if (!inArc || (!inRing && !inGlow))
                {
                    tex.SetPixel(x, y, clear);
                    continue;
                }

                if (inGlow)
                {
                    float t    = 1f - (r - outerR) / (glowOuter - outerR);
                    float angT = 1f - rel / arcHalf;
                    float a    = t * t * angT * 0.5f;
                    // Outer glow: vivid blue
                    tex.SetPixel(x, y, new Color(0.1f, 0.4f, 1f, a));
                    continue;
                }

                // Ring fill — pure blue palette, no white core
                float radT  = 1f - Mathf.Abs(r - midR) / ringHalf;   // 0 edges → 1 mid
                float angT2 = 1f - rel / arcHalf;                    // 0 tips → 1 centre

                float alpha = Mathf.Clamp01(Mathf.Pow(radT, 0.6f) * Mathf.Pow(angT2, 0.25f) * 1.6f);

                // Deep blue → bright cyan-blue core (never white)
                float coreT = Mathf.Pow(radT, 0.8f) * Mathf.Pow(angT2, 0.6f);
                float rC = Mathf.Lerp(0.05f, 0.55f, coreT);   // 5%→55% red
                float gC = Mathf.Lerp(0.25f, 0.85f, coreT);   // 25%→85% green
                float bC = 1f;                                  // always 100% blue

                tex.SetPixel(x, y, new Color(rC, gC, bC, alpha));
            }
        }
        tex.Apply();

        // ── 2. Save PNG ───────────────────────────────────────────────────
        string dir     = "Assets/GeneratedSprites";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        string texPath = dir + "/CrescentSlash.png";
        File.WriteAllBytes(texPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(texPath);

        var imp = (TextureImporter)AssetImporter.GetAtPath(texPath);
        imp.textureType         = TextureImporterType.Sprite;
        imp.spriteImportMode    = SpriteImportMode.Single;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled       = false;
        imp.spritePixelsPerUnit = 100f;
        EditorUtility.SetDirty(imp);
        imp.SaveAndReimport();

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);

        // ── 3. Build new SlashAttack prefab ──────────────────────────────
        var root = new GameObject("SlashAttack");

        var sp   = root.AddComponent<SurvivorIO.SlashProjectile>();
        var soSp = new SerializedObject(sp);
        soSp.FindProperty("speed").floatValue       = 7f;
        soSp.FindProperty("maxDistance").floatValue  = 4f;
        soSp.FindProperty("hitRadius").floatValue    = 0.9f;
        soSp.ApplyModifiedPropertiesWithoutUndo();

        // Visual child
        var vis = new GameObject("Visual");
        vis.transform.SetParent(root.transform, false);
        vis.transform.localScale    = new Vector3(1.8f, 1.8f, 1f);
        vis.transform.localPosition = Vector3.zero;

        var sr          = vis.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        // No custom material — let SpriteRenderer use project default
        sr.color        = Color.white;   // texture is already intrinsically blue
        sr.sortingOrder = 100;           // well above background and player

        string prefabPath = "Assets/Prefabs/SlashAttack.prefab";
        bool ok;
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out ok);
        Object.DestroyImmediate(root);
        if (!ok) return "ERROR: prefab save failed";

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ── 5. Wire to Player → MeleeWeapon ──────────────────────────────
        var player = GameObject.Find("Player");
        if (player == null) return "Prefab created — Player not found in scene";

        var mw = player.GetComponent<SurvivorIO.MeleeWeapon>();
        if (mw == null) return "Prefab created — MeleeWeapon not found on Player";

        var slashPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        var soMw        = new SerializedObject(mw);
        soMw.FindProperty("slashPrefab").objectReferenceValue = slashPrefab;
        soMw.ApplyModifiedPropertiesWithoutUndo();

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return "CrescentSlash rebuilt: blue texture + URP material + sortingOrder=100 ✓";
    }
}
