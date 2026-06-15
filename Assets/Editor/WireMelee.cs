using UnityEngine;
using UnityEditor;

public class WireMelee
{
    public static string Execute()
    {
        var player = GameObject.Find("Player");
        if (player == null) return "Player not found";

        // Add MeleeWeapon if not already present
        var melee = player.GetComponent<SurvivorIO.MeleeWeapon>();
        if (melee == null)
            melee = player.AddComponent<SurvivorIO.MeleeWeapon>();

        // Wire SwordPivot reference via SerializedObject
        var so = new SerializedObject(melee);
        var pivotProp = so.FindProperty("swordPivot");

        var swordPivot = player.transform.Find("main-chibi/SwordPivot");
        if (swordPivot == null) return "SwordPivot not found";

        pivotProp.objectReferenceValue = swordPivot;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Also disable AutoAttackWeapon
        var ranged = player.GetComponent<SurvivorIO.AutoAttackWeapon>();
        if (ranged != null) ranged.enabled = false;

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return $"MeleeWeapon added, SwordPivot={swordPivot.name} wired, AutoAttackWeapon disabled";
    }
}
