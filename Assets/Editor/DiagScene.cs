using UnityEngine;
using UnityEditor;

public class DiagScene
{
    public static string Execute()
    {
        var player = GameObject.Find("Player");
        if (player == null) return "Player not found";
        var chibi = player.transform.Find("main-chibi");
        var sp = chibi?.Find("SwordPivot");
        var excalibur = sp?.Find("Excalibur");

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Player scale: {player.transform.localScale}");
        sb.AppendLine($"main-chibi pos: {chibi?.localPosition} scale: {chibi?.localScale}");
        sb.AppendLine($"SwordPivot pos: {sp?.localPosition} rot: {sp?.localEulerAngles} scale: {sp?.localScale}");
        sb.AppendLine($"Excalibur pos: {excalibur?.localPosition} rot: {excalibur?.localEulerAngles} scale: {excalibur?.localScale}");

        var mw = player.GetComponent<SurvivorIO.MeleeWeapon>();
        if (mw != null)
        {
            var t = mw.GetType();
            var r = t.GetField("range", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)?.GetValue(mw);
            var c = t.GetField("cooldown", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)?.GetValue(mw);
            var d = t.GetField("damage", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)?.GetValue(mw);
            sb.AppendLine($"MeleeWeapon damage={d} range={r} cooldown={c}");
        }

        var enemies = GameObject.FindObjectsByType<SurvivorIO.Enemy>(FindObjectsSortMode.None);
        if (enemies.Length > 0)
        {
            var e = enemies[0];
            var sr = e.GetComponentInChildren<SpriteRenderer>();
            sb.AppendLine($"Enemy[0] scale: {e.transform.localScale} bounds: {sr?.bounds.size}");
        }
        return sb.ToString();
    }
}
