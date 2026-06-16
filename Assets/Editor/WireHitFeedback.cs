using UnityEngine;
using UnityEditor;
using SurvivorIO;

public class WireHitFeedback
{
    [MenuItem("Tools/Wire Hit Feedback")]
    public static void RunFromMenu() { Debug.Log(Execute()); }

    public static string Execute()
    {
        var sb = new System.Text.StringBuilder();

        // 1. Add HitFeedback to every enemy prefab (any prefab with an Enemy component)
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
        int added = 0, skipped = 0;
        foreach (var g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null || go.GetComponent<Enemy>() == null) continue;

            using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                var root = scope.prefabContentsRoot;
                if (root.GetComponent<HitFeedback>() == null)
                {
                    root.AddComponent<HitFeedback>();
                    added++;
                    sb.AppendLine($"  + HitFeedback on {System.IO.Path.GetFileName(path)}");
                }
                else skipped++;
            }
        }
        sb.AppendLine($"Enemy prefabs: added {added}, already had {skipped}");

        // 2. Add HitFeedback to the scene Player (red popup + camera shake, punch the chibi)
        var player = GameObject.Find("Player");
        if (player != null)
        {
            var hf = player.GetComponent<HitFeedback>();
            if (hf == null) hf = player.AddComponent<HitFeedback>();

            var so = new SerializedObject(hf);
            so.FindProperty("shakeOnHit").boolValue       = true;
            so.FindProperty("shakeMagnitude").floatValue  = 0.22f;
            so.FindProperty("popupColor").colorValue      = new Color(1f, 0.35f, 0.35f);
            so.FindProperty("punchAmount").floatValue     = 0.10f;
            // punch the visual chibi, not the root (root has HP/CD bars as children)
            var chibi = FindInChildren(player.transform, "main-chibi");
            if (chibi != null)
                so.FindProperty("punchTarget").objectReferenceValue = chibi;
            so.ApplyModifiedPropertiesWithoutUndo();
            sb.AppendLine("Player HitFeedback wired (shake + red popup) ✓");
        }
        else sb.AppendLine("WARN: Player not found in scene");

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return sb.ToString();
    }

    static Transform FindInChildren(Transform t, string name)
    {
        if (t.name == name) return t;
        foreach (Transform c in t) { var r = FindInChildren(c, name); if (r != null) return r; }
        return null;
    }
}
