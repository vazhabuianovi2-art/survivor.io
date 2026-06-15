using UnityEngine;
using UnityEditor;
using System.IO;

public class FixBackground
{
    public static string Execute()
    {
        // Remove old Quad background if it exists
        var old = GameObject.Find("InfiniteBackground");
        if (old != null) Object.DestroyImmediate(old);

        // Create lava tile texture (64x64)
        const string texPath = "Assets/Materials/LavaTile.png";
        if (!Directory.Exists("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        if (tex == null)
        {
            tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
            {
                float nx = x / 64f;
                float ny = y / 64f;
                // Stone tile: dark borders, slightly lighter center
                float bx = Mathf.Abs(nx - 0.5f) * 2f;
                float by = Mathf.Abs(ny - 0.5f) * 2f;
                float border = Mathf.Max(bx, by);

                Color c;
                if (border > 0.88f)
                    c = new Color(0.06f, 0.04f, 0.04f); // dark crack
                else
                {
                    float heat = 1f - border;
                    // Lava orange glow in cracks, dark stone elsewhere
                    float glow = Mathf.Pow(Mathf.Max(0, heat - 0.7f) * 3.3f, 2f);
                    Color stone = new Color(0.18f, 0.10f, 0.08f);
                    Color lava  = new Color(0.95f, 0.45f, 0.05f);
                    c = Color.Lerp(stone, lava, glow);
                }
                tex.SetPixel(x, y, c);
            }
            tex.Apply();
            System.IO.File.WriteAllBytes(
                System.IO.Path.Combine(Application.dataPath, "../" + texPath),
                tex.EncodeToPNG());
            AssetDatabase.ImportAsset(texPath);
            tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        }

        // Set texture to repeat (wrap mode)
        var ti = AssetImporter.GetAtPath(texPath) as TextureImporter;
        if (ti != null)
        {
            ti.wrapMode = TextureWrapMode.Repeat;
            ti.filterMode = FilterMode.Bilinear;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.SaveAndReimport();
            tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        }

        // Create material with tiling
        const string matPath = "Assets/Materials/LavaBackground.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(mat, matPath);
        }
        mat.mainTexture = tex;

        // Create background SpriteRenderer
        var bgGo = new GameObject("InfiniteBackground");
        var sr = bgGo.AddComponent<SpriteRenderer>();

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);
        if (sprite != null) sr.sprite = sprite;
        sr.color = Color.white;
        sr.sortingOrder = -100;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(200f, 200f);
        sr.tileMode = SpriteTileMode.Continuous;
        bgGo.transform.localScale = Vector3.one;

        bgGo.AddComponent<SurvivorIO.InfiniteBackground>();

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return $"Lava background created with 64x64 tile texture. SpriteRenderer sortingOrder=-100";
    }
}
