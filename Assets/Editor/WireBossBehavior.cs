using UnityEngine;
using UnityEditor;
using SurvivorIO;

public class WireBossBehavior
{
    public static string Execute()
    {
        var goblin = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Goblin.prefab");
        using (var scope = new PrefabUtility.EditPrefabContentsScope("Assets/Prefabs/Enemies/Boss.prefab"))
        {
            var root = scope.prefabContentsRoot;
            var bb = root.GetComponent<BossBehavior>();
            if (bb == null) bb = root.AddComponent<BossBehavior>();
            var so = new SerializedObject(bb);
            so.FindProperty("summonPrefab").objectReferenceValue = goblin;
            so.FindProperty("attackInterval").floatValue = 3f;
            so.FindProperty("projectileDamage").floatValue = 10f;
            so.FindProperty("summonCount").intValue = 3;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        AssetDatabase.SaveAssets();
        return "Boss.prefab → BossBehavior (radial/volley/summon) ✓";
    }
}
