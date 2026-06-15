using UnityEngine;
using UnityEditor;

public class CheckVFX
{
    public static string Execute()
    {
        // Check if Visual Effect Graph is available
        var vfxType = System.Type.GetType("UnityEngine.VFX.VisualEffect, Unity.VisualEffectGraph.Runtime");
        bool vfxAvailable = vfxType != null;

        // Check slash prefab
        string prefabPath = "Assets/Prefabs/SlashAttack.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) return "ERROR: SlashAttack prefab not found";

        var comps = prefab.GetComponentsInChildren<Component>(true);
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("VFX package available: " + vfxAvailable);
        sb.AppendLine("SlashAttack prefab components:");
        foreach (var c in comps)
        {
            sb.AppendLine("  - " + (c != null ? c.GetType().Name : "NULL/Missing"));
        }

        // Check MeleeWeapon slashPrefab reference
        var player = GameObject.Find("Player");
        if (player != null)
        {
            var mw = player.GetComponent<SurvivorIO.MeleeWeapon>();
            if (mw != null)
            {
                var so = new SerializedObject(mw);
                var prop = so.FindProperty("slashPrefab");
                sb.AppendLine("MeleeWeapon.slashPrefab: " + (prop.objectReferenceValue != null ? prop.objectReferenceValue.name : "NULL"));
            }
        }

        return sb.ToString();
    }
}
