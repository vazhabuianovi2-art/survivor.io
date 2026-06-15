using UnityEngine;
using UnityEditor;

public class WireChibiAnim
{
    public static string Execute()
    {
        var player = GameObject.Find("Player");
        if (player == null) return "Player not found";

        var chibi = player.transform.Find("main-chibi");
        if (chibi == null) return "main-chibi not found";

        var animator = chibi.GetComponent<Animator>();
        if (animator == null) return "No Animator on main-chibi";

        var movement = player.GetComponent<SurvivorIO.PlayerMovement>();
        if (movement == null) return "PlayerMovement not found";

        var so = new SerializedObject(movement);
        var prop = so.FindProperty("characterAnimator");
        prop.objectReferenceValue = animator;
        so.ApplyModifiedPropertiesWithoutUndo();

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return $"characterAnimator wired to main-chibi Animator";
    }
}
