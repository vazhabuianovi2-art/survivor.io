using UnityEngine;
using UnityEditor;

public class SetupPlayerBars
{
    public static string Execute()
    {
        var player = GameObject.Find("Player");
        if (player == null) return "ERROR: Player not found";

        // Remove old instance if any
        var old = player.GetComponent<SurvivorIO.PlayerBars>();
        if (old != null) Object.DestroyImmediate(old);

        var bars = player.AddComponent<SurvivorIO.PlayerBars>();
        var so   = new SerializedObject(bars);

        // Wire Health
        var hp = player.GetComponent<SurvivorIO.Health>();
        if (hp != null) so.FindProperty("health").objectReferenceValue = hp;

        // Wire MeleeWeapon
        var mw = player.GetComponent<SurvivorIO.MeleeWeapon>();
        if (mw != null) so.FindProperty("meleeWeapon").objectReferenceValue = mw;

        so.ApplyModifiedPropertiesWithoutUndo();

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return "PlayerBars added to Player. HP=" + (hp != null) + " MW=" + (mw != null);
    }
}
