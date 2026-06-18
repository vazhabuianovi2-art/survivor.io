using UnityEngine;
using UnityEditor;
using SurvivorIO;

public class CreateStageManager
{
    public static string Execute()
    {
        if (Object.FindFirstObjectByType<StageManager>() != null)
            return "StageManager already exists";
        var go = new GameObject("StageManager");
        go.AddComponent<StageManager>();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return "StageManager added ✓";
    }
}
