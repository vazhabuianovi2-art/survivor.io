using UnityEngine;
using UnityEditor;
using System.Linq;

public class AdjustStarwarSword
{
    [MenuItem("Tools/Adjust Starwar Sword")]
    public static void RunFromMenu() { Debug.Log(Execute()); }

    public static string Execute()
    {
        var player = GameObject.Find("Player");
        if (player == null) return "ERROR: Player not found";
        var pivot = FindInChildren(player.transform, "SwordPivot");
        if (pivot == null) return "ERROR: SwordPivot not found";

        // Raise SwordPivot to hand / mid-body level so the swing rotates around the hand
        var p = pivot.localPosition;
        pivot.localPosition = new Vector3(0.35f, 1.9f, -0.1f);

        var sword = pivot.Find("StarwarSword");
        if (sword == null) return "ERROR: StarwarSword not found (run Setup Starwar Sword first)";

        // Handle near the pivot, blade extends outward
        sword.localPosition = new Vector3(0.6f, 0f, 0f);
        sword.localScale    = new Vector3(2.4f, 2.4f, 1f);
        sword.localRotation = Quaternion.identity;

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return $"SwordPivot y: {p.y} -> 1.9, StarwarSword pos=(0.6,0,0) scale=2.4 ✓";
    }

    static Transform FindInChildren(Transform t, string name)
    {
        if (t.name == name) return t;
        foreach (Transform c in t) { var r = FindInChildren(c, name); if (r != null) return r; }
        return null;
    }
}
