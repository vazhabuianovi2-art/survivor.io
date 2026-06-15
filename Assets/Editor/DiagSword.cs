using UnityEngine;
using UnityEditor;
using System.Text;

public class DiagSword
{
    public static string Execute()
    {
        var player = GameObject.Find("Player");
        if (player == null) return "Player not found";

        var pivot = player.transform.Find("main-chibi/SwordPivot");
        if (pivot == null) return "SwordPivot not found";

        var sb = new StringBuilder();
        sb.AppendLine($"SwordPivot localScale={pivot.localScale} lossyScale={pivot.lossyScale} worldPos={pivot.position} rot={pivot.localEulerAngles}");

        var excalibur = pivot.Find("Excalibur");
        if (excalibur != null)
        {
            sb.AppendLine($"Excalibur localPos={excalibur.localPosition} worldPos={excalibur.position}");
            var srs = excalibur.GetComponentsInChildren<SpriteRenderer>(true);
            sb.AppendLine($"SpriteRenderers: {srs.Length}");
            foreach (var sr in srs)
            {
                sb.AppendLine($"  [{sr.name}] active={sr.gameObject.activeInHierarchy} enabled={sr.enabled} color={sr.color} order={sr.sortingOrder}");
                sb.AppendLine($"    worldPos={sr.transform.position} worldBounds={sr.bounds}");
                if (sr.sprite != null)
                    sb.AppendLine($"    sprite={sr.sprite.name} rect={sr.sprite.rect}");
                else
                    sb.AppendLine($"    sprite=NULL");
            }
        }
        return sb.ToString();
    }
}
