using UnityEngine;
using UnityEditor;

public class PreviewSlash
{
    public static string Execute()
    {
        // Cleanup old
        var old = GameObject.Find("__SlashPreview__");
        if (old != null) Object.DestroyImmediate(old);

        string prefabPath = "Assets/Prefabs/SlashAttack.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) return "ERROR: SlashAttack prefab not found";

        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.name = "__SlashPreview__";
        go.transform.position = new Vector3(5f, 0f, 0f);  // far from player
        go.transform.up = Vector2.up;   // face upward so crescent peaks up

        // 2D scene view
        var sv = SceneView.lastActiveSceneView;
        if (sv != null)
        {
            sv.in2DMode = true;
            sv.LookAt(go.transform.position, Quaternion.identity, 3f);
            sv.Repaint();
        }

        Selection.activeGameObject = go;
        return "OK — slash placed at (5,0,0), facing up";
    }

    public static string Cleanup()
    {
        var go = GameObject.Find("__SlashPreview__");
        if (go != null) Object.DestroyImmediate(go);
        return "Cleaned up";
    }
}
