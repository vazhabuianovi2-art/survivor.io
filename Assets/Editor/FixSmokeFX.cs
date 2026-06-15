using UnityEngine;
using UnityEditor;

public class FixSmokeFX
{
    [MenuItem("Tools/Fix Smoke FX Material")]
    public static void RunFromMenu() { Debug.Log(Execute()); }

    public static string Execute()
    {
        var sb = new System.Text.StringBuilder();

        // 1. Create / load a URP particle smoke material
        string matPath = "Assets/GeneratedSprites/SmokePuff.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            // URP particle unlit shader (alpha-blended)
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            mat = new Material(shader);

            // Soft smoke texture
            var smokeTex = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/NamuFX/_CommonAssets/Images/Common/Tex_Smoke.tga");
            if (smokeTex != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", smokeTex);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", smokeTex);
            }
            // alpha-blend surface so smoke is soft, not magenta
            if (mat.HasProperty("_Surface"))    mat.SetFloat("_Surface", 1f);   // Transparent
            if (mat.HasProperty("_Blend"))      mat.SetFloat("_Blend", 0f);     // Alpha
            mat.renderQueue = 3000;

            AssetDatabase.CreateAsset(mat, matPath);
            sb.AppendLine($"Created material: {matPath} (shader={mat.shader.name})");
        }
        else sb.AppendLine($"Material exists: {matPath}");

        // 2. Assign to SmokeDeathFX prefab's ParticleSystemRenderer
        string prefabPath = "Assets/Prefabs/SmokeDeathFX.prefab";
        using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
        {
            var root = scope.prefabContentsRoot;
            int n = 0;
            foreach (var psr in root.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                psr.sharedMaterial = mat;
                psr.material = mat;
                n++;
            }
            sb.AppendLine($"Assigned material to {n} ParticleSystemRenderer(s) ✓");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return sb.ToString();
    }
}
