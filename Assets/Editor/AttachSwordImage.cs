using UnityEngine;
using UnityEditor;
using System.Linq;

public class AttachSwordImage
{
    [MenuItem("Tools/Attach ChatGPT Sword to Player")]
    public static void RunFromMenu() { Debug.Log(Execute()); }

    public static string Execute()
    {
        var sb = new System.Text.StringBuilder();

        // 1. Find ChatGPT Image object in scene (root level)
        var chatGptGo = FindRootByPartialName("ChatGPT");
        if (chatGptGo == null) return "ERROR: ChatGPT Image not found in scene";
        sb.AppendLine($"Found: {chatGptGo.name}");

        // 2. Find Player > main-chibi > SwordPivot
        var player = GameObject.Find("Player");
        if (player == null) return "ERROR: Player not found";

        Transform swordPivot = FindInChildren(player.transform, "SwordPivot");
        if (swordPivot == null) return "ERROR: SwordPivot not found";

        // 3. Remove old generated Lightsaber
        Transform oldLs = swordPivot.Find("Lightsaber");
        if (oldLs != null) { Object.DestroyImmediate(oldLs.gameObject); sb.AppendLine("Removed generated Lightsaber ✓"); }

        // 4. Parent ChatGPT Image under SwordPivot
        chatGptGo.transform.SetParent(swordPivot, false);
        chatGptGo.transform.localPosition = new Vector3(0f, 0f, 0f);
        chatGptGo.transform.localRotation = Quaternion.identity;
        chatGptGo.transform.localScale    = Vector3.one * 0.015f;  // PSB images are huge
        sb.AppendLine("Parented ChatGPT Image under SwordPivot ✓");

        // 5. Fix Layer 0 sorting order — render ON TOP of character (layers are 1-4)
        //    Set to 5 so sword is visible in front of character
        Transform layer0 = chatGptGo.transform.Find("Layer 0");
        if (layer0 != null)
        {
            var sr = layer0.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 5;
                sb.AppendLine($"Layer 0 sortingOrder set to 5 ✓ (sprite: {(sr.sprite != null ? sr.sprite.name : "null")})");
            }
        }

        // 6. Remove bone_1 (not needed, no visual)
        Transform bone = chatGptGo.transform.Find("bone_1");
        if (bone != null) { Object.DestroyImmediate(bone.gameObject); sb.AppendLine("Removed bone_1 ✓"); }

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return sb.ToString();
    }

    static GameObject FindRootByPartialName(string partial)
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (var go in scene.GetRootGameObjects())
            if (go.name.Contains(partial)) return go;
        return null;
    }

    static Transform FindInChildren(Transform t, string name)
    {
        if (t.name == name) return t;
        foreach (Transform c in t) { var r = FindInChildren(c, name); if (r != null) return r; }
        return null;
    }
}
