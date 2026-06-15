using UnityEngine;
using UnityEditor;

public class RebuildScene
{
    public static string Execute()
    {
        var log = new System.Text.StringBuilder();

        // ── 1. Import lava texture with FullRect (required for Tiled SpriteRenderer) ──
        string texPath = "Assets/Reference/lava infinity.png";
        var imp = (TextureImporter)AssetImporter.GetAtPath(texPath);
        if (imp == null) return "ERROR: lava infinity.png not found";

        imp.textureType      = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.wrapMode         = TextureWrapMode.Repeat;
        imp.filterMode       = FilterMode.Bilinear;
        imp.mipmapEnabled    = false;
        imp.spritePixelsPerUnit = 100f;

        // Set FullRect via TextureImporterSettings (required for Tiled draw mode)
        var settings = new TextureImporterSettings();
        imp.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        imp.SetTextureSettings(settings);

        EditorUtility.SetDirty(imp);
        imp.SaveAndReimport();
        log.AppendLine("lava infinity.png → Sprite FullRect Repeat ✓");

        var lavaSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);
        if (lavaSprite == null) return "ERROR: sprite not loaded";

        // ── 2. Rebuild InfiniteBackground as SpriteRenderer Tiled ────────
        var oldBg = GameObject.Find("InfiniteBackground");
        if (oldBg != null) Object.DestroyImmediate(oldBg);

        var bgGo = new GameObject("InfiniteBackground");
        bgGo.transform.position   = new Vector3(0f, 0f, 1f);
        bgGo.transform.localScale = Vector3.one;

        var sr          = bgGo.AddComponent<SpriteRenderer>();
        sr.sprite       = lavaSprite;
        sr.drawMode     = SpriteDrawMode.Tiled;
        sr.tileMode     = SpriteTileMode.Continuous;
        sr.size         = new Vector2(500f, 500f);
        sr.sortingOrder = -100;
        sr.color        = Color.white;

        bgGo.AddComponent<SurvivorIO.InfiniteBackground>();
        log.AppendLine("InfiniteBackground SpriteRenderer Tiled 500x500 static ✓");

        // ── 3. Restore blue crescent slash ────────────────────────────────
        string crescentPath = "Assets/Prefabs/SlashAttack.prefab";
        var crescentPrefab  = AssetDatabase.LoadAssetAtPath<GameObject>(crescentPath);

        var player = GameObject.Find("Player");
        if (player != null && crescentPrefab != null)
        {
            var mw = player.GetComponent<SurvivorIO.MeleeWeapon>();
            if (mw != null)
            {
                var so = new SerializedObject(mw);
                so.FindProperty("slashPrefab").objectReferenceValue = crescentPrefab;
                so.FindProperty("range").floatValue = 5f;
                so.ApplyModifiedPropertiesWithoutUndo();
                log.AppendLine("SlashAttack.prefab (blue crescent) wired, range=5 ✓");
            }
        }

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return log.ToString();
    }
}
