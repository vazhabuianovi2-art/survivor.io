using UnityEngine;

public class VerifyMenu
{
    public static string Execute()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"timeScale={Time.timeScale}");

        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return "no canvas";

        Report(canvas.transform, "MainMenuPanel", sb);
        Report(canvas.transform, "ShopPanel", sb);
        Report(canvas.transform, "StagePanel", sb);
        Report(canvas.transform, "CharPanel", sb);
        Report(canvas.transform, "Weapons", sb);   // WeaponHUD label
        sb.AppendLine($"canvas childCount={canvas.transform.childCount}");
        return sb.ToString();
    }

    static void Report(Transform canvas, string name, System.Text.StringBuilder sb)
    {
        var t = Find(canvas, name);
        if (t == null) { sb.AppendLine($"{name}: NOT FOUND"); return; }
        sb.AppendLine($"{name}: active={t.gameObject.activeInHierarchy} siblingIndex={t.GetSiblingIndex()} parent={t.parent.name}");
    }

    static Transform Find(Transform root, string name)
    {
        if (root.name == name) return root;
        foreach (Transform c in root) { var r = Find(c, name); if (r != null) return r; }
        return null;
    }
}
