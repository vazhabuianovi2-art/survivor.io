using UnityEngine;
using UnityEditor;

public class DiagAndFix
{
    public static string Execute()
    {
        var sb = new System.Text.StringBuilder();

        // Check player HP
        var player = GameObject.Find("Player");
        if (player == null) return "Player not found";
        var hp = player.GetComponent<SurvivorIO.Health>();
        sb.AppendLine($"Player maxHealth={hp?.Max} current={hp?.Current}");

        // Boost player HP to 200 for testing
        if (hp != null)
        {
            var so = new SerializedObject(hp);
            so.FindProperty("maxHealth").floatValue = 200f;
            so.ApplyModifiedPropertiesWithoutUndo();
            sb.AppendLine("Set player maxHealth=200");
        }

        // Lower WaveManager unlock times for quick test
        var wm = Object.FindFirstObjectByType<SurvivorIO.WaveManager>();
        if (wm != null)
        {
            var so = new SerializedObject(wm);
            so.FindProperty("miniBossTime").floatValue = 30f;
            so.FindProperty("bossTime").floatValue = 60f;

            var waves = so.FindProperty("waves");
            // Wave 0 = Goblin, unlockTime stays 0
            // Wave 1 = Orc, unlock at 10s
            if (waves.arraySize > 1)
                waves.GetArrayElementAtIndex(1).FindPropertyRelative("unlockTime").floatValue = 10f;
            // Wave 2 = Skeleton, unlock at 10s
            if (waves.arraySize > 2)
                waves.GetArrayElementAtIndex(2).FindPropertyRelative("unlockTime").floatValue = 10f;
            // Wave 3 = Bat, unlock at 20s
            if (waves.arraySize > 3)
                waves.GetArrayElementAtIndex(3).FindPropertyRelative("unlockTime").floatValue = 20f;
            so.ApplyModifiedPropertiesWithoutUndo();
            sb.AppendLine("Wave unlock times lowered for testing (Orc/Skel=10s, Bat=20s, MiniBoss=30s, Boss=60s)");
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return sb.ToString();
    }
}
