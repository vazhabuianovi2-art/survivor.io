using System.IO;
using UnityEditor;
using UnityEngine;

// Temporary editor utility (lives outside Assets/ so it is not part of the
// normal project compilation). Run via Coplay execute_script to generate simple
// circular placeholder sprites until real art is added.
public static class PlaceholderSpriteGen
{
    public static void Execute()
    {
        const int size = 256;
        string dir = "Assets/Art";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        // name, fill colour, outline colour
        CreateCircle(dir, "player", size, new Color(0.20f, 0.45f, 1f), new Color(0.05f, 0.12f, 0.35f));
        // plain white circle for UI tinting (joystick bg/handle, etc.)
        CreateCircle(dir, "uicircle", size, Color.white, new Color(1f, 1f, 1f, 0f));
        // tiling ground grid so player movement is visible
        CreateGridTile(dir, "grid", size, new Color(0.34f, 0.34f, 0.40f), new Color(0.28f, 0.28f, 0.34f));
        // hollow ring for the joystick background (survivor.io style outer ring)
        CreateRing(dir, "uiring", size, Color.white, 0.10f);

        AssetDatabase.Refresh();
        Debug.Log("PlaceholderSpriteGen: done.");
    }

    private static void CreateCircle(string dir, string name, int size, Color fill, Color outline)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float r = size * 0.5f;
        float outerR = r - 4f;          // leave a small transparent margin
        float outlineW = size * 0.06f;  // outline thickness

        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - r + 0.5f;
                float dy = y - r + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                Color c;
                if (dist > outerR)
                    c = Color.clear;
                else if (dist > outerR - outlineW)
                    c = outline;
                else
                    c = fill;

                // simple top-left highlight for a bit of depth
                if (c == fill && dist < outerR - outlineW)
                {
                    float h = Mathf.Clamp01(1f - (new Vector2(dx + r * 0.3f, dy - r * 0.3f).magnitude) / outerR);
                    c = Color.Lerp(c, Color.white, h * 0.25f);
                }

                pixels[y * size + x] = c;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();

        string path = $"{dir}/{name}.png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = size; // 256px -> 1 world unit
        importer.filterMode = FilterMode.Bilinear;
        importer.SaveAndReimport();
    }

    // Hollow ring: transparent centre, solid coloured band near the edge.
    // ringWidth is a fraction of the radius (e.g. 0.10 = 10% thick band).
    private static void CreateRing(string dir, string name, int size, Color color, float ringWidth)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float r = size * 0.5f;
        float outerR = r - 4f;
        float bandW = outerR * ringWidth;
        float innerR = outerR - bandW;

        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - r + 0.5f;
                float dy = y - r + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                // anti-aliased band edges
                float a = Mathf.Clamp01(outerR - dist) * Mathf.Clamp01(dist - innerR);
                a = (dist <= outerR && dist >= innerR) ? 1f : Mathf.Clamp01(a);
                Color c = color;
                c.a = (dist <= outerR && dist >= innerR) ? color.a : 0f;
                pixels[y * size + x] = c;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        SaveSprite(dir, name, tex, size);
    }

    // Shared importer setup for simple single sprites.
    private static void SaveSprite(string dir, string name, Texture2D tex, int size)
    {
        string path = $"{dir}/{name}.png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = size;
        importer.filterMode = FilterMode.Bilinear;
        importer.SaveAndReimport();
    }

    private static void CreateGridTile(string dir, string name, int size, Color baseColor, Color lineColor)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        int lineW = Mathf.Max(2, size / 64);
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool onLine = x < lineW || y < lineW || x >= size - lineW || y >= size - lineW;
                pixels[y * size + x] = onLine ? lineColor : baseColor;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();

        string path = $"{dir}/{name}.png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = size;          // one tile = 1 world unit
        importer.wrapMode = TextureWrapMode.Repeat;
        importer.filterMode = FilterMode.Point;

        // FullRect mesh is required for SpriteRenderer tiled draw mode.
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);

        importer.SaveAndReimport();
    }
}
