using UnityEngine;
using UnityEditor;

public class FixSwordVis
{
    public static string Execute()
    {
        var player = GameObject.Find("Player");
        if (player == null) return "Player not found";

        var pivot = player.transform.Find("main-chibi/SwordPivot");
        if (pivot == null) return "SwordPivot not found";

        pivot.localScale = new Vector3(3.0f, 3.0f, 1f);
        pivot.localEulerAngles = Vector3.zero;
        // Right-side waist of character
        pivot.localPosition = new Vector3(0.5f, 0.7f, -0.1f);

        var srs = pivot.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in srs)
        {
            sr.sortingOrder = 5;
            sr.color = Color.white;
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        return $"scale=3, rot=0, pos=(0.5,0.7,-0.1), {srs.Length} sprites reset";
    }
}
