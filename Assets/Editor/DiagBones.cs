using UnityEngine;
using UnityEditor;
using System.Text;

public class DiagBones
{
    public static string Execute()
    {
        var player = GameObject.Find("Player");
        if (player == null) return "Player not found";
        var chibi = player.transform.Find("main-chibi");
        if (chibi == null) return "main-chibi not found";

        var sb = new StringBuilder();
        PrintTree(chibi, sb, 0);
        return sb.ToString();
    }

    static void PrintTree(Transform t, StringBuilder sb, int depth)
    {
        string indent = new string(' ', depth * 2);
        var sr = t.GetComponent<SpriteRenderer>();
        string extra = sr != null ? " [SR]" : "";
        var anim = t.GetComponent<Animator>();
        extra += anim != null ? " [Animator]" : "";
        sb.AppendLine($"{indent}{t.name}  localPos={t.localPosition:F3}  localRot={t.localEulerAngles:F1}{extra}");
        foreach (Transform c in t)
            PrintTree(c, sb, depth + 1);
    }
}
