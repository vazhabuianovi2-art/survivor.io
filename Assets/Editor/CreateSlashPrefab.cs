using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateSlashPrefab
{
    public static string Execute()
    {
        // ── 1. Generate crescent texture ──────────────────────────────────
        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Color clear = new Color(0, 0, 0, 0);
        Color blue  = new Color(0.3f, 0.7f, 1.0f, 1.0f);
        Color white = new Color(0.85f, 0.95f, 1.0f, 1.0f);

        float cx = size * 0.5f;
        float cy = size * 0.5f;
        float outerR = size * 0.48f;
        float innerR = size * 0.28f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float r = Mathf.Sqrt(dx * dx + dy * dy);

                // Crescent: ring between innerR and outerR, angle 30°..150° (top arc)
                float angleDeg = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                // Rotate so arc faces upward (toward positive Y = travel direction)
                // Arc spans from -80° to +80° centered on +Y (90°)
                float relAngle = Mathf.Abs(Mathf.DeltaAngle(angleDeg, 90f));

                if (r >= innerR && r <= outerR && relAngle <= 80f)
                {
                    // Fade edges
                    float radialT = 1f - Mathf.Abs((r - (innerR + outerR) * 0.5f) / ((outerR - innerR) * 0.5f));
                    float angularT = 1f - (relAngle / 80f);
                    float alpha = Mathf.Clamp01(radialT * radialT * angularT * 1.8f);

                    // White core, blue edges
                    float coreT = Mathf.Clamp01(radialT * 2f - 0.5f);
                    Color col = Color.Lerp(blue, white, coreT);
                    col.a = alpha;
                    tex.SetPixel(x, y, col);
                }
                else
                {
                    tex.SetPixel(x, y, clear);
                }
            }
        }
        tex.Apply();

        // ── 2. Save texture ───────────────────────────────────────────────
        string texDir = "Assets/GeneratedSprites";
        if (!Directory.Exists(texDir)) Directory.CreateDirectory(texDir);
        string texPath = texDir + "/SlashEffect.png";
        File.WriteAllBytes(texPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(texPath);

        // Set to Sprite import mode
        var imp = (TextureImporter)AssetImporter.GetAtPath(texPath);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = false;
        imp.filterMode = FilterMode.Bilinear;
        EditorUtility.SetDirty(imp);
        imp.SaveAndReimport();

        // ── 3. Create Material (additive blend for glow effect) ───────────
        string matPath = texDir + "/SlashMat.mat";
        Material mat;
        if (!File.Exists(matPath))
        {
            mat = new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(mat, matPath);
        }
        else
        {
            mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        }
        var slashSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);
        mat.mainTexture = slashSprite.texture;
        // Additive blend
        mat.SetFloat("_SrcBlend", 1f);  // One
        mat.SetFloat("_DstBlend", 1f);  // One
        EditorUtility.SetDirty(mat);

        // ── 4. Create prefab ──────────────────────────────────────────────
        string prefabDir = "Assets/Prefabs";
        if (!Directory.Exists(prefabDir)) Directory.CreateDirectory(prefabDir);
        string prefabPath = prefabDir + "/SlashEffect.prefab";

        var go = new GameObject("SlashEffect");

        // SpriteRenderer
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = slashSprite;
        sr.material = mat;
        sr.sortingOrder = 10;
        go.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

        // SlashProjectile script
        var sp = go.AddComponent<SurvivorIO.SlashProjectile>();
        // Set default values via SerializedObject
        var so = new SerializedObject(sp);
        so.FindProperty("speed").floatValue = 7f;
        so.FindProperty("maxDistance").floatValue = 3f;
        so.FindProperty("hitRadius").floatValue = 0.6f;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Save as prefab
        bool success;
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath, out success);
        Object.DestroyImmediate(go);

        if (!success) return "ERROR: prefab save failed";

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ── 5. Wire SlashEffect prefab into Player's MeleeWeapon ─────────
        var player = GameObject.Find("Player");
        if (player != null)
        {
            var mw = player.GetComponent<SurvivorIO.MeleeWeapon>();
            if (mw != null)
            {
                var slashGo = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                var mwSo = new SerializedObject(mw);
                mwSo.FindProperty("slashPrefab").objectReferenceValue = slashGo;
                mwSo.ApplyModifiedPropertiesWithoutUndo();
                UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
                return "SlashEffect prefab created + wired to MeleeWeapon on Player";
            }
            return "SlashEffect prefab created — MeleeWeapon not found on Player";
        }
        return "SlashEffect prefab created — Player not found in scene";
    }
}
