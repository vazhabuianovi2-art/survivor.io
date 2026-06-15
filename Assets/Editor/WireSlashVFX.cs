using UnityEngine;
using UnityEditor;

public class WireSlashVFX
{
    public static string Execute()
    {
        // ── 1. Load the blue VFX slash prefab ────────────────────────────
        string vfxPath = "Assets/slash5-HungNguyen/prefab/slash/white-blue bolder.prefab";
        var vfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(vfxPath);
        if (vfxPrefab == null) return "ERROR: could not load " + vfxPath;

        // ── 2. Build a wrapper: SlashAttack (script) + VFX child ─────────
        var root = new GameObject("SlashAttack");

        // Add SlashProjectile (movement + damage logic)
        var sp = root.AddComponent<SurvivorIO.SlashProjectile>();
        var soSp = new SerializedObject(sp);
        soSp.FindProperty("speed").floatValue = 7f;
        soSp.FindProperty("maxDistance").floatValue = 3f;
        soSp.FindProperty("hitRadius").floatValue = 0.6f;
        soSp.ApplyModifiedPropertiesWithoutUndo();

        // Instantiate VFX as child (visuals only)
        var vfxGo = (GameObject)PrefabUtility.InstantiatePrefab(vfxPrefab);
        vfxGo.transform.SetParent(root.transform, false);
        vfxGo.transform.localPosition = Vector3.zero;
        vfxGo.transform.localRotation = Quaternion.identity;

        // ── 3. Save as new prefab ────────────────────────────────────────
        string prefabPath = "Assets/Prefabs/SlashAttack.prefab";
        bool ok;
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out ok);
        Object.DestroyImmediate(root);
        if (!ok) return "ERROR: prefab save failed";

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ── 4. Wire to Player's MeleeWeapon ─────────────────────────────
        var player = GameObject.Find("Player");
        if (player == null) return "Prefab created — Player not found in scene";

        var mw = player.GetComponent<SurvivorIO.MeleeWeapon>();
        if (mw == null) return "Prefab created — MeleeWeapon not found on Player";

        var slashPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        var soMw = new SerializedObject(mw);
        soMw.FindProperty("slashPrefab").objectReferenceValue = slashPrefab;
        soMw.ApplyModifiedPropertiesWithoutUndo();

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return "SlashAttack prefab (white-blue VFX) wired to MeleeWeapon on Player";
    }
}
