using UnityEngine;
using UnityEngine.UI;

namespace SurvivorIO
{
    /// <summary>
    /// Offers a one-time revive when the player dies (Survivor.io style): heal to
    /// full and clear the screen, or give up and end the run. Built in code.
    /// </summary>
    [DefaultExecutionOrder(95)]
    public class ReviveUI : MonoBehaviour
    {
        public static ReviveUI Instance { get; private set; }

        private Canvas _canvas;
        private Font _font;
        private GameObject _panel;
        private Health _playerHealth;
        private bool _used;

        public bool CanRevive => !_used;

        private void Awake()
        {
            Instance = this;
            _used = false;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _canvas = FindFirstObjectByType<Canvas>();
            if (_canvas == null) return;
            Build();
            _panel.SetActive(false);
        }

        /// <summary>Show the revive prompt. Returns false if a revive isn't available.</summary>
        public bool Offer(Health playerHealth)
        {
            if (_used) return false;
            _playerHealth = playerHealth;
            _panel.SetActive(true);
            _panel.transform.SetAsLastSibling();
            Time.timeScale = 0f;
            return true;
        }

        private void DoRevive()
        {
            _used = true;
            _panel.SetActive(false);
            Time.timeScale = 1f;

            if (_playerHealth != null) _playerHealth.Revive(_playerHealth.Max);

            // Screen clear so the player gets breathing room.
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                foreach (var c in Physics2D.OverlapCircleAll(player.transform.position, 30f))
                    if (c.CompareTag("Enemy")) c.GetComponent<Health>()?.TakeDamage(99999f);
            }
            if (CameraFollow.Instance != null) CameraFollow.Instance.Shake(0.4f, 0.3f);
        }

        private void GiveUp()
        {
            _panel.SetActive(false);
            Time.timeScale = 1f;
            if (GameManager.Instance != null) GameManager.Instance.TriggerGameOver();
        }

        private void Build()
        {
            _panel = new GameObject("RevivePanel", typeof(RectTransform));
            _panel.transform.SetParent(_canvas.transform, false);
            var rt = _panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            _panel.AddComponent<Image>().color = new Color(0.1f, 0.02f, 0.04f, 0.88f);

            var title = MakeLabel("YOU DIED", 76, new Vector2(0f, 240f), new Vector2(800f, 120f));
            title.fontStyle = FontStyle.Bold; title.color = new Color(0.95f, 0.3f, 0.3f);

            var sub = MakeLabel("Revive and clear the screen?", 38, new Vector2(0f, 120f), new Vector2(800f, 70f));
            sub.color = new Color(1f, 0.85f, 0.4f);

            MakeButton("REVIVE", new Vector2(0f, -20f), new Vector2(460f, 120f), 50,
                new Color(0.2f, 0.55f, 0.3f), DoRevive);
            MakeButton("GIVE UP", new Vector2(0f, -170f), new Vector2(460f, 100f), 40,
                new Color(0.4f, 0.25f, 0.28f), GiveUp);
        }

        private void MakeButton(string text, Vector2 pos, Vector2 size, int fs, Color color,
            UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("Btn_" + text, typeof(RectTransform));
            go.transform.SetParent(_panel.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            var img = go.AddComponent<Image>(); img.color = color;
            var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);
            MakeChildLabel(go.transform, text, fs);
        }

        private Text MakeChildLabel(Transform parent, string text, int fs)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var t = go.AddComponent<Text>();
            t.font = _font; t.text = text; t.fontSize = fs; t.alignment = TextAnchor.MiddleCenter;
            t.fontStyle = FontStyle.Bold; t.color = Color.white; t.raycastTarget = false;
            return t;
        }

        private Text MakeLabel(string text, int size, Vector2 pos, Vector2 sd)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(_panel.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = sd;
            var t = go.AddComponent<Text>();
            t.font = _font; t.text = text; t.fontSize = size; t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white; t.raycastTarget = false;
            return t;
        }
    }
}
