using UnityEngine;
using UnityEditor;
using SurvivorIO;

public class ForceBossCombat
{
    public static string Execute()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return "ERROR: player not found (playing?)";
        Vector3 p = player.transform.position;

        var wm = Object.FindFirstObjectByType<WeaponManager>();
        if (wm != null) { var lt = wm.Acquire(typeof(LightningEmitterWeapon)); lt.LevelUp(); lt.LevelUp(); lt.LevelUp(); lt.LevelUp(); }

        var boss = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Boss.prefab");
        if (boss != null) Object.Instantiate(boss, p + new Vector3(0f, 5f, 0f), Quaternion.identity);

        var goblin = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Goblin.prefab");
        if (goblin != null)
            for (int i = 0; i < 10; i++)
                Object.Instantiate(goblin, p + new Vector3(Mathf.Cos(i) * 2f, Mathf.Sin(i) * 2f, 0f) + new Vector3(2.5f, 0f, 0f), Quaternion.identity);

        return "Spawned boss + 10 goblins + Lightning L5";
    }
}
