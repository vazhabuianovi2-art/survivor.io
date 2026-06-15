using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class CreateLightsaber
{
    [MenuItem("Tools/Create Lightsaber + Fix Purple Slash")]
    public static void RunFromMenu() { Debug.Log(Execute()); }

    public static string Execute()
    {
        var log = new System.Text.StringBuilder();

        // ── 1. Generate lightsaber blade texture ─────────────────────────
        int W = 32, H = 128;
        var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                float cx   = (x - W * 0.5f) / (W * 0.5f);  // -1..1
                float dist = Mathf.Abs(cx);

                // tip fade (top 15% gets dimmer)
                float tipY = (float)y / H;
                float tipFade = tipY > 0.85f ? Mathf.InverseLerp(1f, 0.85f, tipY) : 1f;

                // white core (inner 15% of width)
                float core  = Mathf.Clamp01(1f - dist / 0.15f);
                // purple glow (inner 70%)
                float glow  = Mathf.Clamp01(1f - dist / 0.70f);
                glow = Mathf.Pow(glow, 1.5f);

                float alpha = Mathf.Max(core, glow * 0.9f) * tipFade;
                float r = Mathf.Lerp(glow * 0.7f, 1f, core);
                float g = Mathf.Lerp(0f,           1f, core) * 0.2f;
                float b = 1f;

                tex.SetPixel(x, y, new Color(r, g, b, alpha));
            }
        }
        tex.Apply();

        string dir    = "Assets/GeneratedSprites";
        string saPath = dir + "/Lightsaber.png";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllBytes(saPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(saPath, ImportAssetOptions.ForceUpdate);

        var imp = (TextureImporter)AssetImporter.GetAtPath(saPath);
        imp.textureType         = TextureImporterType.Sprite;
        imp.spriteImportMode    = SpriteImportMode.Single;
        imp.filterMode          = FilterMode.Bilinear;
        imp.mipmapEnabled       = false;
        imp.alphaIsTransparency = true;
        imp.spritePixelsPerUnit = 100f;
        imp.wrapMode            = TextureWrapMode.Clamp;
        var ts = new TextureImporterSettings();
        imp.ReadTextureSettings(ts);
        ts.spriteMeshType = SpriteMeshType.FullRect;
        imp.SetTextureSettings(ts);
        imp.SaveAndReimport();

        var saSprite = AssetDatabase.LoadAssetAtPath<Sprite>(saPath);
        if (saSprite == null) return "ERROR: lightsaber sprite failed";
        log.AppendLine("Lightsaber texture generated ✓");

        // ── 2. Replace Excalibur under SwordPivot ────────────────────────
        var player = GameObject.Find("Player");
        if (player == null) return "ERROR: Player not found in scene";

        // Find SwordPivot recursively
        Transform swordPivot = FindInChildren(player.transform, "SwordPivot");
        if (swordPivot == null) return "ERROR: SwordPivot not found";

        // Remove Excalibur child
        Transform excalibur = swordPivot.Find("Excalibur");
        if (excalibur != null)
        {
            Object.DestroyImmediate(excalibur.gameObject);
            log.AppendLine("Excalibur removed ✓");
        }

        // Create Lightsaber object
        var lsGo = new GameObject("Lightsaber");
        lsGo.transform.SetParent(swordPivot, false);
        // Position: blade starts at pivot, extends upward in local space
        lsGo.transform.localPosition = new Vector3(0.45f, 0.3f, 0f);
        lsGo.transform.localRotation = Quaternion.Euler(0f, 0f, -45f);
        lsGo.transform.localScale    = new Vector3(1.2f, 1.8f, 1f);

        var sr = lsGo.AddComponent<SpriteRenderer>();
        sr.sprite       = saSprite;
        sr.color        = Color.white;
        sr.sortingOrder = 15;
        log.AppendLine("Lightsaber added to SwordPivot ✓");

        // ── 3. Fix purple slash in SlashAttack.prefab ────────────────────
        string slash2Path = "Assets/Reference/slash2.png";
        var slashImp = (TextureImporter)AssetImporter.GetAtPath(slash2Path);
        if (slashImp != null)
        {
            // Keep existing Multiple slice (was set by SetupPurpleSlash)
            var sprites = AssetDatabase.LoadAllAssetsAtPath(slash2Path)
                                       .OfType<Sprite>().ToArray();
            if (sprites.Length > 0)
            {
                var purpleSprite = sprites[0];
                string prefabPath = "Assets/Prefabs/SlashAttack.prefab";
                using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
                {
                    var root = scope.prefabContentsRoot;
                    var slashSr = root.GetComponentInChildren<SpriteRenderer>(true);
                    if (slashSr != null)
                    {
                        slashSr.sprite       = purpleSprite;
                        slashSr.color        = Color.white;
                        slashSr.sortingOrder = 100;
                        log.AppendLine($"Purple slash sprite set on '{slashSr.gameObject.name}' ✓");
                    }
                }
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        return log.ToString();
    }

    static Transform FindInChildren(Transform t, string name)
    {
        if (t.name == name) return t;
        foreach (Transform c in t)
        {
            var r = FindInChildren(c, name);
            if (r != null) return r;
        }
        return null;
    }
}
