using UnityEngine;
using UnityEditor;
using UnityEngine.U2D.Animation;
using System.Linq;

public class FixSwordAndSlash
{
    [MenuItem("Tools/Fix Sword + Slash")]
    public static void RunFromMenu() { Debug.Log(Execute()); }

    public static string Execute()
    {
        var sb = new System.Text.StringBuilder();

        // ── 1. Remove SpriteSkin from the star war sword (fixes invisible in Play) ──
        var player = GameObject.Find("Player");
        if (player == null) return "ERROR: Player not found";

        var pivot = FindInChildren(player.transform, "SwordPivot");
        if (pivot == null) return "ERROR: SwordPivot not found";

        int removed = 0;
        foreach (var skin in pivot.GetComponentsInChildren<SpriteSkin>(true))
        {
            Object.DestroyImmediate(skin);
            removed++;
        }
        sb.AppendLine($"Removed {removed} SpriteSkin component(s) from sword ✓");

        // ── 2. Replace slash projectile sprite with slash.psb "Layer 1" ──
        string slashPsb = "Assets/Reference/slash.psb";
        var slashSprite = AssetDatabase.LoadAllAssetsAtPath(slashPsb)
                                       .OfType<Sprite>()
                                       .FirstOrDefault();
        if (slashSprite == null) return sb.ToString() + "\nERROR: no sprite in slash.psb";

        string prefabPath = "Assets/Prefabs/SlashAttack.prefab";
        using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
        {
            var root = scope.prefabContentsRoot;
            var sr = root.GetComponentInChildren<SpriteRenderer>(true);
            if (sr != null)
            {
                sr.sprite       = slashSprite;
                sr.color        = Color.white;
                sr.sortingOrder = 100;
                // PSB is 455x436 @100PPU ≈ 4.5 units — scale down the Visual
                sr.transform.localScale = Vector3.one * 0.5f;
                sb.AppendLine($"Slash sprite set to '{slashSprite.name}' on '{sr.gameObject.name}' ✓");
            }
            else sb.AppendLine("ERROR: no SpriteRenderer in SlashAttack prefab");
        }

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
