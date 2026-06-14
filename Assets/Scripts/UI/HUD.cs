using UnityEngine;
using UnityEngine.UI;

namespace SurvivorIO
{
    /// <summary>
    /// In-game heads-up display: XP bar + level, survival timer, kill count and
    /// the player HP bar. Built programmatically and refreshed every frame.
    /// </summary>
    public class HUD : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;

        private PlayerExperience _xp;
        private Health _playerHealth;

        private RectTransform _xpFill;
        private RectTransform _hpFill;
        private Text _levelText;
        private Text _timerText;
        private Text _killsText;
        private Font _font;

        private void Awake()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
            BuildUI();
        }

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _xp = player.GetComponent<PlayerExperience>();
                _playerHealth = player.GetComponent<Health>();
            }
        }

        private void Update()
        {
            if (_xp != null)
            {
                float frac = _xp.XpToNext > 0f ? Mathf.Clamp01(_xp.Xp / _xp.XpToNext) : 0f;
                SetFill(_xpFill, frac);
                _levelText.text = $"Lv.{_xp.Level}";
            }

            if (_playerHealth != null)
            {
                float frac = _playerHealth.Max > 0f ? Mathf.Clamp01(_playerHealth.Current / _playerHealth.Max) : 0f;
                SetFill(_hpFill, frac);
            }

            if (GameManager.Instance != null)
            {
                int t = Mathf.FloorToInt(GameManager.Instance.ElapsedTime);
                _timerText.text = $"{t / 60:00}:{t % 60:00}";
            }

            _killsText.text = $"Kills  {Enemy.KillCount}";
        }

        private static void SetFill(RectTransform fill, float frac)
        {
            var max = fill.anchorMax;
            max.x = frac;
            fill.anchorMax = max;
        }

        // ---------- UI construction ----------

        private void BuildUI()
        {
            // XP bar across the very top.
            _xpFill = CreateBar("XPBar", -8f, 26f,
                new Color(0.1f, 0.12f, 0.16f, 0.9f), new Color(0.45f, 0.9f, 0.3f));

            // HP bar just below it.
            _hpFill = CreateBar("HPBar", -40f, 20f,
                new Color(0.1f, 0.12f, 0.16f, 0.9f), new Color(0.9f, 0.25f, 0.25f));

            // Level (left), Timer (center), Kills (right).
            _levelText = CreateLabel("Level", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -72f), 36, TextAnchor.UpperLeft, "Lv.1");
            _levelText.color = new Color(0.7f, 1f, 0.5f);

            _timerText = CreateLabel("Timer", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -72f), 48, TextAnchor.UpperCenter, "00:00");
            _timerText.fontStyle = FontStyle.Bold;

            _killsText = CreateLabel("Kills", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-24f, -72f), 34, TextAnchor.UpperRight, "Kills  0");
            _killsText.color = new Color(1f, 0.7f, 0.7f);
        }

        private RectTransform CreateBar(string name, float y, float height, Color bgColor, Color fillColor)
        {
            var bg = CreateChild(name, canvas.transform);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0f, 1f);
            bgRt.anchorMax = new Vector2(1f, 1f);
            bgRt.pivot = new Vector2(0.5f, 1f);
            bgRt.sizeDelta = new Vector2(-40f, height); // 20px margin each side
            bgRt.anchoredPosition = new Vector2(0f, y);
            bg.AddComponent<Image>().color = bgColor;

            var fill = CreateChild("Fill", bg.transform);
            var fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = new Vector2(0f, 0f);
            fillRt.anchorMax = new Vector2(1f, 1f);
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            fill.AddComponent<Image>().color = fillColor;
            return fillRt;
        }

        private Text CreateLabel(string name, Vector2 aMin, Vector2 aMax, Vector2 pos,
            int size, TextAnchor anchor, string content)
        {
            var go = CreateChild(name, canvas.transform);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.pivot = new Vector2(anchor == TextAnchor.UpperLeft ? 0f : anchor == TextAnchor.UpperRight ? 1f : 0.5f, 1f);
            rt.sizeDelta = new Vector2(400f, 60f);
            rt.anchoredPosition = pos;

            var text = go.AddComponent<Text>();
            text.font = _font;
            text.text = content;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static GameObject CreateChild(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }
    }
}
