using UnityEngine;
using UnityEditor;
using SurvivorIO;

public class CleanStrayWeapons
{
    public static string Execute()
    {
        var player = GameObject.Find("Player");
        if (player == null) return "ERROR: Player not found";

        // Weapons (WeaponBase-derived) must only be added at runtime via WeaponManager.
        // Any serialized on the Player are strays that crash (Owner not serialized → null).
        var strays = player.GetComponents<WeaponBase>();
        int n = 0;
        foreach (var w in strays)
        {
            Object.DestroyImmediate(w, true);
            n++;
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return $"Removed {n} stray WeaponBase component(s) from Player ✓";
    }
}
