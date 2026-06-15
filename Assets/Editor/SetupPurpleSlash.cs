using UnityEngine;
using UnityEditor;
using System.Linq;

public class SetupPurpleSlash
{
    [UnityEditor.MenuItem("Tools/Setup Purple Slash")]
    public static void RunFromMenu() { UnityEngine.Debug.Log(Execute()); }

    public static string Execute()
    {
        string slash2Path = "Assets/Reference/slash2.png";

        // ── 1. Import as readable single to get real pixel dimensions ──────
        var imp = (TextureImporter)AssetImporter.GetAtPath(slash2Path);
        if (imp == null) return "ERROR: slash2.png not found in Assets/Reference/";

        imp.textureType         = TextureImporterType.Sprite;
        imp.spriteImportMode    = SpriteImportMode.Single;
        imp.alphaIsTransparency = true;
        imp.filterMode          = FilterMode.Bilinear;
        imp.mipmapEnabled       = false;
        imp.spritePixelsPerUnit = 100f;
        imp.isReadable          = true;
        imp.wrapMode            = TextureWrapMode.Clamp;

        var ts = new TextureImporterSettings();
        imp.ReadTextureSettings(ts);
        ts.spriteMeshType = SpriteMeshType.FullRect;
        imp.SetTextureSettings(ts);
        imp.SaveAndReimport();

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(slash2Path);
        if (tex == null) return "ERROR: Texture2D not loaded";

        int tw = tex.width;
        int th = tex.height;

        // ── 2. Slice: 3 columns, 2 rows — grab top-middle (crescent arc) ──
        // slash2.png layout: [stroke | crescent | ring] on top, [spiral | swirl] below
        int cols = 3, rows = 2;
        int cw   = tw / cols;
        int ch   = th / rows;

        // Unity sprite rects: y=0 is BOTTOM of image
        // top row in Unity coords starts at ch (= th/2)
        int sx = cw;  // column 1 (middle)
        int sy = ch;  // top row

        imp.spriteImportMode = SpriteImportMode.Multiple;
        imp.spritesheet = new SpriteMetaData[]
        {
            new SpriteMetaData
            {
                name      = "PurpleSlash_Crescent",
                rect      = new Rect(sx, sy, cw, ch),
                alignment = (int)SpriteAlignment.Center,
                pivot     = new Vector2(0.5f, 0.5f)
            }
        };
        imp.SaveAndReimport();

        // ── 3. Load the sliced sprite ──────────────────────────────────────
        var sprite = AssetDatabase.LoadAllAssetsAtPath(slash2Path)
                                  .OfType<Sprite>()
                                  .FirstOrDefault();
        if (sprite == null)
            return $"ERROR: no sprite found after slicing (tex={tw}x{th}, rect={sx},{sy},{cw},{ch})";

        // ── 4. Update SlashAttack prefab ───────────────────────────────────
        string prefabPath = "Assets/Prefabs/SlashAttack.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
            return "ERROR: SlashAttack.prefab not found";

        string result = "";
        using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
        {
            var root = scope.prefabContentsRoot;
            var sr   = root.GetComponentInChildren<SpriteRenderer>(true);
            if (sr != null)
            {
                sr.sprite       = sprite;
                sr.color        = Color.white;
                sr.sortingOrder = 100;
                result = $"SpriteRenderer found on '{sr.gameObject.name}' ✓";
            }
            else
            {
                result = "ERROR: no SpriteRenderer found in prefab children";
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return $"Purple crescent slash ready! tex={tw}x{th}, cell={cw}x{ch}\n{result}";
    }
}
