using UnityEngine;
using UnityEditor;
using SurvivorIO;

public class CreatePauseUI
{
    public static string Execute()
    {
        if (Object.FindFirstObjectByType<PauseUI>() != null)
            return "PauseUI already exists";
        var go = new GameObject("PauseUI");
        go.AddComponent<PauseUI>();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return "PauseUI added ✓";
    }
}
