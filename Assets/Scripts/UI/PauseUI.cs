using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SurvivorIO
{
    /// <summary>
    /// In-run pause: a small button that freezes the game and shows Resume /
    /// Main Menu. Quitting to menu banks the run's gold and reloads the scene
    /// (which brings the MainMenuUI back up).
    /// </summary>
    [DefaultExecutionOrder(90)]
    public class PauseUI : MonoBehaviour
    {
        private Canvas _canvas;
        private Font _font;
        private GameObject _panel;
        private Button _pauseButton;

        private void Awake()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _canvas = FindFirstObjectByType<Canvas>();
            if (_canvas == null) return;
            Build();
            _panel.SetActive(false);
        }

        private void Pause()
        {
            if (GameManager.Instance != null &&
                (GameManager.Instance.IsGameOver || GameManager.Instance.IsGameWon)) return;
            _panel.SetActive(true);
            _panel.transform.SetAsLastSibling();
            Time.timeScale = 0f;
        }

        private void Resume()
        {
            _panel.SetActive(false);
            Time.timeScale = 1f;
        }

        private void QuitToMenu()
        {
            if (GameManager.Instance != null)
                MetaProgress.BankRunGold(GameManager.Instance.Gold);
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void Build()
        {
            // small pause button, top-right under the gold counter
            _pauseButton = MakeButton(_canvas.transform, "II", new Vector2(1f, 1f),
                new Vector2(-30f, -150f), new Vector2(70f, 70f), 34,
                new Color(0.15f, 0.17f, 0.24f, 0.9f), Pause);
            _pauseButton.targetGraphic.GetComponent<RectTransform>().pivot = new Vector2(1f, 1f);

            _panel = new GameObject("PausePanel", typeof(RectTransform));
            _panel.transform.SetParent(_canvas.transform, false);
            var rt = _panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            _panel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);

            var title = MakeLabel(_panel.transform, "PAUSED", 64, new Vector2(0.5f, 0.5f),
                new Vector2(0f, 200f), new Vector2(600f, 100f));
            title.fontStyle = FontStyle.Bold; title.color = new Color(1f, 0.85f, 0.3f);

            MakeButton(_panel.transform, "RESUME", new Vector2(0.5f, 0.5f), new Vector2(0f, 40f),
                new Vector2(440f, 110f), 44, new Color(0.2f, 0.5f, 0.25f), Resume);
            MakeButton(_panel.transform, "MAIN MENU", new Vector2(0.5f, 0.5f), new Vector2(0f, -100f),
                new Vector2(440f, 110f), 40, new Color(0.4f, 0.25f, 0.3f), QuitToMenu);
        }

        private Button MakeButton(Transform parent, string text, Vector2 anchor, Vector2 pos,
            Vector2 size, int fontSize, Color color, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("Btn_" + text, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>(); img.color = color;
            var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);
            var label = MakeLabel(go.transform, text, fontSize, new Vector2(0.5f, 0.5f), Vector2.zero, size);
            label.fontStyle = FontStyle.Bold;
            return btn;
        }

        private Text MakeLabel(Transform parent, string text, int size, Vector2 anchor, Vector2 pos, Vector2 sd)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = sd;
            var t = go.AddComponent<Text>();
            t.font = _font; t.text = text; t.fontSize = size; t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white; t.raycastTarget = false;
            return t;
        }
    }
}
