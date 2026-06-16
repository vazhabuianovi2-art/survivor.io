using UnityEngine;
using UnityEditor;
using SurvivorIO;

public class WireWeaponManager
{
    public static string Execute()
    {
        var player = GameObject.Find("Player");
        if (player == null) return "ERROR: Player not found";

        var wm = player.GetComponent<WeaponManager>();
        if (wm == null) wm = player.AddComponent<WeaponManager>();

        var projectile = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile.prefab");
        var so = new SerializedObject(wm);
        so.FindProperty("projectilePrefab").objectReferenceValue = projectile;
        so.FindProperty("maxWeapons").intValue = 5;
        so.ApplyModifiedPropertiesWithoutUndo();

        // PlayerStats (global multiplier hub) + PassiveManager (passive items)
        if (player.GetComponent<PlayerStats>() == null)    player.AddComponent<PlayerStats>();
        if (player.GetComponent<PassiveManager>() == null) player.AddComponent<PassiveManager>();

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return $"WeaponManager + PlayerStats + PassiveManager wired on Player (projectile={(projectile != null ? "set" : "NULL")}) ✓";
    }
}
