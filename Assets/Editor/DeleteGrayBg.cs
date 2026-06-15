using UnityEngine;
using UnityEditor;

public class DeleteGrayBg
{
    public static string Execute()
    {
        var bg = GameObject.Find("Background");
        if (bg != null)
        {
            Object.DestroyImmediate(bg);
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            return "Background deleted ✓";
        }
        return "Background object not found";
    }
}
