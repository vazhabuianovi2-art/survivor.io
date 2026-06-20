using UnityEngine;
using UnityEditor;
using SurvivorIO;

public class CreateHeroPortrait
{
    public static string Execute()
    {
        if (Object.FindFirstObjectByType<HeroPortrait>() != null)
            return "HeroPortrait already exists";
        var go = new GameObject("HeroPortrait");
        go.AddComponent<HeroPortrait>();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return "HeroPortrait added ✓";
    }
}
