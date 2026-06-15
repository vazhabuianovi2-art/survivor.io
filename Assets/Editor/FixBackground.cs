using UnityEngine;
using UnityEditor;
using System.IO;

public class FixBackground
{
    public static string Execute()
    {
        string lavaTexPath = "Assets/Reference/lava infinity.png";

        // Fix texture import: FullRect mesh required for Tiled SpriteRenderer
        var imp = (TextureImporter)AssetImporter.GetAtPath(lavaTexPath);
        if (imp == null) return "ERROR: lava infinity.png not found";

        imp.textureType         = TextureImporterType.Sprite;
        imp.spriteImportMode    = SpriteImportMode.Single;
        imp.wrapMode            = TextureWrapMode.Repeat;
        imp.filterMode          = FilterMode.Bilinear;
        imp.mipmapEnabled       = false;
        imp.spritePixelsPerUnit = 100f;
        imp.alphaIsTransparency = false;
        EditorUtility.SetDirty(imp);
        imp.SaveAndReimport();

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(lavaTexPath);
        if (sprite == null) return "ERROR: sprite not loaded after reimport";

        var bg = GameObject.Find("InfiniteBackground");
        if (bg == null) return "ERROR: InfiniteBackground not found";

        var sr = bg.GetComponent<SpriteRenderer>();
        if (sr == null) return "ERROR: no SpriteRenderer on InfiniteBackground";

        sr.sprite       = sprite;
        sr.drawMode     = SpriteDrawMode.Tiled;
        sr.tileMode     = SpriteTileMode.Continuous;
        bg.transform.localScale = Vector3.one;
        sr.size         = new Vector2(80f, 80f);
        sr.sortingOrder = -100;

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return "Background fixed: FullRect tiled lava ground ✓";
    }
}
