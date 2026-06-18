using UnityEngine;
using UnityEditor;
using SurvivorIO;

public class CreateReviveUI
{
    public static string Execute()
    {
        if (Object.FindFirstObjectByType<ReviveUI>() != null)
            return "ReviveUI already exists";
        var go = new GameObject("ReviveUI");
        go.AddComponent<ReviveUI>();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return "ReviveUI added ✓";
    }
}
