using UnityEngine;
using UnityEngine.UI;

namespace SurvivorIO
{
    /// <summary>
    /// Kill-streak combo: consecutive kills within a time window build a combo
    /// that boosts gold gain and shows a counter. Resets if you stop killing.
    /// </summary>
    [DefaultExecutionOrder(80)]
    public class ComboManager : MonoBehaviour
    {
        public static ComboManager Instance { get; private set; }

        [SerializeField] private float window = 3f;

        private int _combo;
        private float _timer;
        private Text _text;
        private Font _font;

        /// <summary>Gold multiplier from the current combo (up to +100% at combo 50).</summary>
        public static float GoldMult =>
            Instance != null ? 1f + Mathf.Min(Instance._combo, 50) * 0.02f : 1f;

        private void Awake()
        {
            Instance = this;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            var go = new GameObject("Combo", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -130f);
            rt.sizeDelta = new Vector2(600f, 70f);
            _text = go.AddComponent<Text>();
            _text.font = _font; _text.fontSize = 44; _text.alignment = TextAnchor.UpperCenter;
            _text.fontStyle = FontStyle.Bold; _text.raycastTarget = false;
            _text.text = "";
        }

        private void OnEnable()  => Enemy.Killed += OnKill;
        private void OnDisable() => Enemy.Killed -= OnKill;

        private void OnKill()
        {
            _combo++;
            _timer = window;
            Refresh();
        }

        private void Update()
        {
            if (_combo <= 0) return;
            _timer -= Time.deltaTime;
            if (_timer <= 0f) { _combo = 0; Refresh(); }
        }

        private void Refresh()
        {
            if (_text == null) return;
            if (_combo < 2) { _text.text = ""; return; }
            _text.text = $"x{_combo} COMBO";
            // warm up the colour as the streak grows
            float h = Mathf.Lerp(0.14f, 0f, Mathf.Min(_combo, 30) / 30f); // yellow → red
            _text.color = Color.HSVToRGB(h, 0.85f, 1f);
        }
    }
}
