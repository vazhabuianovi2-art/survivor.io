using UnityEngine;
using UnityEditor;
using SurvivorIO;

public class CreateComboManager
{
    public static string Execute()
    {
        if (Object.FindFirstObjectByType<ComboManager>() != null)
            return "ComboManager already exists";
        var go = new GameObject("ComboManager");
        go.AddComponent<ComboManager>();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return "ComboManager added ✓";
    }
}
