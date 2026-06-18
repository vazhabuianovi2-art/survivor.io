using UnityEngine;
using UnityEditor;
using SurvivorIO;

public class CreateMainMenu
{
    public static string Execute()
    {
        if (Object.FindFirstObjectByType<MainMenuUI>() == null)
        {
            var go = new GameObject("MainMenu");
            go.AddComponent<MainMenuUI>();
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        }

        // Demo gold so the shop is testable.
        MetaProgress.Data.gold = 500;
        MetaProgress.Save();

        return "MainMenuUI added + demo gold 500 ✓";
    }
}
