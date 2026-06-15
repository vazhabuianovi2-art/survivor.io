using UnityEngine;
using UnityEditor;
using System.IO;

public class FixBackground2
{
    public static string Execute()
    {
        // Remove old background
        var old = GameObject.Find("InfiniteBackground");
        if (old != null) Object.DestroyImmediate(old);

        // Set camera background to dark lava color
        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.06f, 0.04f); // dark lava
        }

        // Create background with a small tile sprite repeated via material UV
        const string matPath = "Assets/Materials/LavaBackground.mat";
        const string texPath = "Assets/Materials/LavaTile.png";

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);

        var bgGo = new GameObject("InfiniteBackground");
        bgGo.transform.position = new Vector3(0, 0, 1f);
        bgGo.transform.localScale = new Vector3(60f, 100f, 1f);

        var sr = bgGo.AddComponent<SpriteRenderer>();
        if (sprite != null) sr.sprite = sprite;
        sr.color = Color.white;
        sr.sortingOrder = -100;
        // Use Simple mode to avoid vertex explosion; scale handles coverage
        sr.drawMode = SpriteDrawMode.Simple;

        // Use a material with tiling
        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(mat, matPath);
        }
        if (tex != null)
        {
            mat.mainTexture = tex;
            mat.mainTextureScale = new Vector2(30f, 50f);
        }
        else
        {
            mat.color = new Color(0.18f, 0.09f, 0.05f);
        }
        sr.sharedMaterial = mat;

        bgGo.AddComponent<SurvivorIO.InfiniteBackground>();

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return $"Background fixed: camera bgcolor=dark lava, sprite scale=60x100, material tiling=30x50";
    }
}
