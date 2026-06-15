using UnityEngine;
using UnityEngine.UI;

namespace SurvivorIO
{
    public class BossHealthBar : MonoBehaviour
    {
        private static BossHealthBar _instance;
        public static BossHealthBar Instance => _instance;

        private GameObject _panel;
        private Image _fill;
        private Text _label;
        private Health _bossHealth;
        private Font _font;

        private void Awake()
        {
            _instance = this;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null) BuildUI(canvas.transform);
        }

        public void SetBoss(Health bossHealth, string bossName = "BOSS")
        {
            _bossHealth = bossHealth;
            if (_panel != null) _panel.SetActive(true);
            if (_label != null) _label.text = bossName;
            if (_bossHealth != null)
            {
                _bossHealth.Died += _ => HideBar();
                _bossHealth.Damaged += (h, _) => UpdateFill(h);
            }
            UpdateFill(_bossHealth);
        }

        private void HideBar()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        private void UpdateFill(Health h)
        {
            if (_fill != null && h != null)
                _fill.fillAmount = h.Current / h.Max;
        }

        private void BuildUI(Transform canvasRoot)
        {
            _panel = new GameObject("BossHealthBarPanel", typeof(RectTransform));
            _panel.transform.SetParent(canvasRoot, false);

            var rt = _panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, 0f);
            rt.anchorMax = new Vector2(0.9f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, 30f);
            rt.sizeDelta = new Vector2(0, 60f);

            // Background
            var bg = new GameObject("BG", typeof(RectTransform));
            bg.transform.SetParent(_panel.transform, false);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
            bg.AddComponent<Image>().color = new Color(0.1f, 0.05f, 0.05f, 0.9f);

            // Fill
            var fillGo = new GameObject("Fill", typeof(RectTransform));
            fillGo.transform.SetParent(_panel.transform, false);
            var fillRt = fillGo.GetComponent<RectTransform>();
            fillRt.anchorMin = new Vector2(0f, 0.1f);
            fillRt.anchorMax = new Vector2(1f, 0.9f);
            fillRt.offsetMin = new Vector2(4f, 0); fillRt.offsetMax = new Vector2(-4f, 0);
            _fill = fillGo.AddComponent<Image>();
            _fill.color = new Color(0.9f, 0.15f, 0.15f);
            _fill.type = Image.Type.Filled;
            _fill.fillMethod = Image.FillMethod.Horizontal;
            _fill.fillOrigin = 0;
            _fill.fillAmount = 1f;

            // Label
            var labelGo = new GameObject("BossLabel", typeof(RectTransform));
            labelGo.transform.SetParent(_panel.transform, false);
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero; labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero; labelRt.offsetMax = Vector2.zero;
            _label = labelGo.AddComponent<Text>();
            _label.font = _font;
            _label.text = "BOSS";
            _label.fontSize = 32;
            _label.alignment = TextAnchor.MiddleCenter;
            _label.color = Color.white;
            _label.fontStyle = FontStyle.Bold;
            _label.raycastTarget = false;

            _panel.SetActive(false);
        }
    }
}
