using UnityEngine;
using UnityEditor;

public class FixSwordScale
{
    public static string Execute()
    {
        var player = GameObject.Find("Player");
        if (player == null) return "Player not found";
        var chibi = player.transform.Find("main-chibi");
        if (chibi == null) return "main-chibi not found";
        var sp = chibi.Find("SwordPivot");
        if (sp == null) return "SwordPivot not found";

        // Scale sword down to match enemy size (~0.52 world units, enemy=0.43)
        sp.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        // Keep pivot close to body center (slightly right of center)
        sp.localPosition = new Vector3(0.35f, 0.20f, -0.1f);

        // Adjust MeleeWeapon range to match tighter visual
        var mw = player.GetComponent<SurvivorIO.MeleeWeapon>();
        if (mw != null)
        {
            var so = new SerializedObject(mw);
            so.FindProperty("range").floatValue = 1.0f;
            so.FindProperty("swingStartAngle").floatValue = 60f;
            so.FindProperty("swingEndAngle").floatValue = -60f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return $"SwordPivot scale=1.0, pos=(0.35,0.20,-0.1), range=1.0, swing 60 to -60";
    }
}
