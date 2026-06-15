using UnityEngine;
using UnityEditor;

public class SetProductionValues
{
    public static string Execute()
    {
        var sb = new System.Text.StringBuilder();

        // Player HP — 150 for balanced gameplay
        var player = GameObject.Find("Player");
        if (player != null)
        {
            var hp = player.GetComponent<SurvivorIO.Health>();
            if (hp != null)
            {
                var so = new SerializedObject(hp);
                so.FindProperty("maxHealth").floatValue = 150f;
                so.ApplyModifiedPropertiesWithoutUndo();
                sb.AppendLine("Player maxHealth=150");
            }
        }

        // WaveManager — production timings
        var wm = Object.FindFirstObjectByType<SurvivorIO.WaveManager>();
        if (wm != null)
        {
            var so = new SerializedObject(wm);
            so.FindProperty("miniBossTime").floatValue = 300f;   // 5 min
            so.FindProperty("bossTime").floatValue = 900f;        // 15 min

            var waves = so.FindProperty("waves");
            // Wave 0 = Goblin: starts immediately
            if (waves.arraySize > 0)
                waves.GetArrayElementAtIndex(0).FindPropertyRelative("unlockTime").floatValue = 0f;
            // Wave 1 = Orc: 3 min
            if (waves.arraySize > 1)
                waves.GetArrayElementAtIndex(1).FindPropertyRelative("unlockTime").floatValue = 180f;
            // Wave 2 = Skeleton: 3 min
            if (waves.arraySize > 2)
                waves.GetArrayElementAtIndex(2).FindPropertyRelative("unlockTime").floatValue = 180f;
            // Wave 3 = Bat: 5 min
            if (waves.arraySize > 3)
                waves.GetArrayElementAtIndex(3).FindPropertyRelative("unlockTime").floatValue = 300f;

            so.ApplyModifiedPropertiesWithoutUndo();
            sb.AppendLine("WaveManager: Goblin=0s, Orc/Skel=3min, Bat=5min, MiniBoss=5min, Boss=15min");
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return sb.ToString();
    }
}
