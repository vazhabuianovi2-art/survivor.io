using UnityEngine;
using UnityEngine.UI;

namespace SurvivorIO
{
    /// <summary>
    /// Defeat screen shown when the player dies: title, survived time, kills and
    /// a Retry button that restarts the run. Built programmatically and hidden
    /// until <see cref="GameManager.GameOver"/> fires.
    /// </summary>
    public class DefeatUI : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;

        private GameObject _panel;
        private Text _statsText;
        private Font _font;

        private void Awake()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
            BuildUI();
            _panel.SetActive(false);
        }

        private void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.GameOver += OnGameOver;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.GameOver -= OnGameOver;
        }

        private void OnGameOver()
        {
            int t = Mathf.FloorToInt(GameManager.Instance.ElapsedTime);
            int level = 1;
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var xp = player.GetComponent<PlayerExperience>();
                if (xp != null) level = xp.Level;
            }
            _statsText.text =
                $"Survived  {t / 60:00}:{t % 60:00}\n" +
                $"Kills  {Enemy.KillCount}\n" +
                $"Level  {level}\n" +
                $"Gold earned  {GameManager.Instance.Gold}";
            _panel.SetActive(true);
        }

        private void BuildUI()
        {
            _panel = CreateChild("DefeatPanel", canvas.transform);
            var rt = _panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            _panel.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.08f, 0.9f);

            var title = CreateText("Title", _panel.transform, "DEFEAT", 80, TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            title.color = new Color(0.9f, 0.3f, 0.3f);
            Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 320f), new Vector2(800f, 120f));

            var sub = CreateText("Sub", _panel.transform, "Keep trying!", 44, TextAnchor.MiddleCenter);
            sub.color = new Color(1f, 0.85f, 0.3f);
            Place(sub.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 200f), new Vector2(800f, 80f));

            _statsText = CreateText("Stats", _panel.transform, "", 40, TextAnchor.MiddleCenter);
            _statsText.color = Color.white;
            Place(_statsText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(800f, 160f));

            // Retry button
            var btnGo = CreateChild("RetryButton", _panel.transform);
            var btnRt = btnGo.GetComponent<RectTransform>();
            Place(btnRt, new Vector2(0.5f, 0.5f), new Vector2(0f, -180f), new Vector2(440f, 130f));
            var img = btnGo.AddComponent<Image>();
            img.color = new Color(0.95f, 0.75f, 0.2f);
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() =>
            {
                if (GameManager.Instance != null) GameManager.Instance.Retry();
            });

            var label = CreateText("Label", btnGo.transform, "RETRY", 48, TextAnchor.MiddleCenter);
            label.fontStyle = FontStyle.Bold;
            label.color = new Color(0.1f, 0.1f, 0.12f);
            var lblRt = label.rectTransform;
            lblRt.anchorMin = Vector2.zero;
            lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = Vector2.zero;
            lblRt.offsetMax = Vector2.zero;
        }

        private static void Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        private Text CreateText(string name, Transform parent, string content, int size, TextAnchor anchor)
        {
            var go = CreateChild(name, parent);
            var text = go.AddComponent<Text>();
            text.font = _font;
            text.text = content;
            text.fontSize = size;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
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
