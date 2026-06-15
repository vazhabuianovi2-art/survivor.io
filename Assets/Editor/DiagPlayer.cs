using UnityEngine;
using UnityEditor;

public class DiagPlayer
{
    public static string Execute()
    {
        var player = GameObject.Find("Player");
        if (player == null) return "Player not found";

        var sb = new System.Text.StringBuilder();
        foreach (var c in player.GetComponents<MonoBehaviour>())
            sb.AppendLine($"  {c.GetType().Name} enabled={c.enabled}");
        return sb.ToString();
    }
}
