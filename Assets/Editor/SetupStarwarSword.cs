using UnityEngine;
using UnityEditor;
using System.Linq;

public class SetupStarwarSword
{
    [MenuItem("Tools/Setup Starwar Sword")]
    public static void RunFromMenu() { Debug.Log(Execute()); }

    public static string Execute()
    {
        var sb = new System.Text.StringBuilder();

        // 1. Load the starwar sprite (plain Sprite, no rig)
        string path = "Assets/Reference/starwar.psb";
        var sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
        if (sprite == null) return "ERROR: no sprite in starwar.psb";
        sb.AppendLine($"Loaded sprite '{sprite.name}' {sprite.rect.width}x{sprite.rect.height} ppu={sprite.pixelsPerUnit}");

        // 2. Find SwordPivot
        var player = GameObject.Find("Player");
        if (player == null) return "ERROR: Player not found";
        var pivot = FindInChildren(player.transform, "SwordPivot");
        if (pivot == null) return "ERROR: SwordPivot not found";

        // 3. Clear ALL existing children of SwordPivot (old ChatGPT Image / lightsaber)
        var toDelete = new System.Collections.Generic.List<GameObject>();
        foreach (Transform c in pivot) toDelete.Add(c.gameObject);
        foreach (var g in toDelete) Object.DestroyImmediate(g);
        sb.AppendLine($"Cleared {toDelete.Count} old sword child(ren) ✓");

        // 4. Create fresh sword with a PLAIN SpriteRenderer (no SpriteSkin)
        var swordGo = new GameObject("StarwarSword");
        swordGo.transform.SetParent(pivot, false);
        // sprite native size at PPU 1500 ≈ 0.71 x 0.23 units. Scale up to a usable sword.
        swordGo.transform.localScale    = new Vector3(2.4f, 2.4f, 1f);
        // offset so the blade extends outward from the hand
        swordGo.transform.localPosition = new Vector3(0.55f, 0f, 0f);
        swordGo.transform.localRotation = Quaternion.identity;

        var sr = swordGo.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.color        = Color.white;
        sr.sortingOrder = 5;   // in front of character layers (1-4)
        sb.AppendLine("Created StarwarSword with plain SpriteRenderer ✓");

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
