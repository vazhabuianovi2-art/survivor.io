using UnityEngine;
using UnityEngine.UI;

namespace SurvivorIO
{
    public class WinUI : MonoBehaviour
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
                GameManager.Instance.GameWon += OnGameWon;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.GameWon -= OnGameWon;
        }

        private void OnGameWon()
        {
            int t = Mathf.FloorToInt(GameManager.Instance.ElapsedTime);
            _statsText.text = $"Time  {t / 60:00}:{t % 60:00}\nKills  {Enemy.KillCount}";
            _panel.SetActive(true);
        }

        private void BuildUI()
        {
            _panel = CreateChild("WinPanel", canvas.transform);
            var rt = _panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            _panel.AddComponent<Image>().color = new Color(0.04f, 0.12f, 0.04f, 0.92f);

            var title = CreateText("Title", _panel.transform, "YOU WIN!", 90, TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            title.color = new Color(0.3f, 1f, 0.4f);
            Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 320f), new Vector2(900f, 140f));

            var sub = CreateText("Sub", _panel.transform, "Boss Defeated!", 50, TextAnchor.MiddleCenter);
            sub.color = new Color(1f, 0.9f, 0.2f);
            Place(sub.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 200f), new Vector2(800f, 80f));

            _statsText = CreateText("Stats", _panel.transform, "", 40, TextAnchor.MiddleCenter);
            _statsText.color = Color.white;
            Place(_statsText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(800f, 160f));

            var btnGo = CreateChild("RetryButton", _panel.transform);
            Place(btnGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0f, -180f), new Vector2(440f, 130f));
            var img = btnGo.AddComponent<Image>();
            img.color = new Color(0.2f, 0.8f, 0.3f);
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() =>
            {
                if (GameManager.Instance != null) GameManager.Instance.Retry();
            });

            var label = CreateText("Label", btnGo.transform, "PLAY AGAIN", 44, TextAnchor.MiddleCenter);
            label.fontStyle = FontStyle.Bold;
            label.color = new Color(0.05f, 0.1f, 0.05f);
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
