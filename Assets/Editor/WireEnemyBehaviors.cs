using UnityEngine;
using UnityEditor;
using SurvivorIO;

public class WireEnemyBehaviors
{
    public static string Execute()
    {
        var sb = new System.Text.StringBuilder();

        AddBehavior<ChargerBehavior>("Assets/Prefabs/Enemies/Orc.prefab", sb);
        AddBehavior<RangedBehavior>("Assets/Prefabs/Enemies/Skeleton.prefab", sb);
        AddBehavior<ExploderBehavior>("Assets/Prefabs/Enemies/Bat.prefab", sb);

        // MiniBoss splits into a few goblins on death.
        var goblin = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Goblin.prefab");
        using (var scope = new PrefabUtility.EditPrefabContentsScope("Assets/Prefabs/Enemies/MiniBoss.prefab"))
        {
            var root = scope.prefabContentsRoot;
            var sp = root.GetComponent<SplitterBehavior>();
            if (sp == null) sp = root.AddComponent<SplitterBehavior>();
            var so = new SerializedObject(sp);
            so.FindProperty("spawnPrefab").objectReferenceValue = goblin;
            so.FindProperty("count").intValue = 4;
            so.FindProperty("scale").floatValue = 0.7f;
            so.ApplyModifiedPropertiesWithoutUndo();
            sb.AppendLine("MiniBoss → SplitterBehavior (4 goblins) ✓");
        }

        AssetDatabase.SaveAssets();
        return sb.ToString();
    }

    private static void AddBehavior<T>(string path, System.Text.StringBuilder sb) where T : Component
    {
        using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
        {
            var root = scope.prefabContentsRoot;
            if (root.GetComponent<T>() == null) root.AddComponent<T>();
            sb.AppendLine($"{System.IO.Path.GetFileName(path)} → {typeof(T).Name} ✓");
        }
    }
}
