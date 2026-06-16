using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SurvivorIO
{
    /// <summary>
    /// Bottom-left list of the player's current build: the sword plus every
    /// equipped weapon and passive with its level. Refreshed each frame from the
    /// WeaponManager / PassiveManager.
    /// </summary>
    public class WeaponHUD : MonoBehaviour
    {
        private WeaponManager _weapons;
        private PassiveManager _passives;
        private Text _weaponText;
        private Text _passiveText;
        private Font _font;
        private readonly StringBuilder _sb = new StringBuilder();

        private void Awake()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            _weaponText  = CreateLabel(canvas.transform, "Weapons", new Vector2(16f, 140f),
                new Color(1f, 0.95f, 0.7f));
            _passiveText = CreateLabel(canvas.transform, "Passives", new Vector2(16f, 16f),
                new Color(0.75f, 0.9f, 1f));
        }

        private void Start()
        {
            _weapons  = FindFirstObjectByType<WeaponManager>();
            _passives = FindFirstObjectByType<PassiveManager>();
        }

        private void Update()
        {
            if (_weaponText != null)
            {
                _sb.Clear();
                _sb.AppendLine("<b>WEAPONS</b>");
                _sb.AppendLine("Sword");
                if (_weapons != null)
                    foreach (var w in _weapons.Weapons)
                        _sb.AppendLine($"{w.DisplayName}  Lv{w.Level}");
                _weaponText.text = _sb.ToString();
            }

            if (_passiveText != null)
            {
                _sb.Clear();
                _sb.AppendLine("<b>PASSIVES</b>");
                if (_passives != null)
                    foreach (var p in _passives.Owned)
                        _sb.AppendLine($"{p.DisplayName}  Lv{p.Level}");
                _passiveText.text = _sb.ToString();
            }
        }

        private Text CreateLabel(Transform parent, string name, Vector2 anchoredBottomLeft, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0f, 0f);
            rt.sizeDelta = new Vector2(260f, 120f);
            rt.anchoredPosition = anchoredBottomLeft;

            var text = go.AddComponent<Text>();
            text.font = _font;
            text.fontSize = 22;
            text.alignment = TextAnchor.LowerLeft;
            text.color = color;
            text.supportRichText = true;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }
    }
}
