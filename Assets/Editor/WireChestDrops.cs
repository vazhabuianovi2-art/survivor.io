using UnityEngine;
using UnityEditor;
using SurvivorIO;

public class WireChestDrops
{
    public static string Execute()
    {
        string[] paths = { "Assets/Prefabs/Enemies/Boss.prefab", "Assets/Prefabs/Enemies/MiniBoss.prefab" };
        var sb = new System.Text.StringBuilder();
        foreach (var path in paths)
        {
            using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                var enemy = scope.prefabContentsRoot.GetComponent<Enemy>();
                if (enemy != null)
                {
                    var so = new SerializedObject(enemy);
                    so.FindProperty("dropsChest").boolValue = true;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    sb.AppendLine($"{System.IO.Path.GetFileName(path)} → dropsChest=true ✓");
                }
                else sb.AppendLine($"{System.IO.Path.GetFileName(path)} → no Enemy component");
            }
        }
        AssetDatabase.SaveAssets();
        return sb.ToString();
    }
}
