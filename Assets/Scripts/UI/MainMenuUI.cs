using UnityEngine;
using UnityEngine.UI;

namespace SurvivorIO
{
    /// <summary>
    /// Front-end menu shown before a run (game frozen at timeScale 0). Lets the
    /// player start the run, spend banked gold on permanent upgrades (Shop) and
    /// pick a character. Built programmatically on the existing Canvas.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class MainMenuUI : MonoBehaviour
    {
        private Canvas _canvas;
        private Font _font;

        private GameObject _main, _shop, _chars, _stages, _settings;
        private Text _goldMain, _goldShop, _goldChars, _goldStages;
        private Text _sfxVal, _musVal;
        private readonly Text[] _shopRows = new Text[6];
        private readonly Text[] _charRows = new Text[3];
        private readonly Text[] _stageRows = new Text[3];

        private void Awake()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _canvas = FindFirstObjectByType<Canvas>();
            if (_canvas == null) return;

            BuildMain();
            BuildShop();
            BuildChars();
            BuildStages();
            BuildSettings();

            ShowOnly(_main);
            Time.timeScale = 0f;     // freeze the run until Play
        }

        // ---------------- actions ----------------

        private void Play()
        {
            // Re-apply meta so menu changes (upgrades/character) take effect.
            if (PlayerStats.Instance != null) PlayerStats.Instance.ApplyMeta();
            _main.SetActive(false);
            _shop.SetActive(false);
            _chars.SetActive(false);
            _stages.SetActive(false);
            _settings.SetActive(false);
            Time.timeScale = 1f;
        }

        private void RefreshGold()
        {
            string g = $"Gold: {MetaProgress.Gold}";
            if (_goldMain) _goldMain.text = g;
            if (_goldShop) _goldShop.text = g;
            if (_goldChars) _goldChars.text = g;
            if (_goldStages) _goldStages.text = g;
        }

        private void BuyUpgrade(int id)
        {
            MetaProgress.Buy(id);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayLevelUp();
            RefreshShop();
        }

        private void PickCharacter(int idx)
        {
            if (MetaProgress.IsCharacterUnlocked(idx)) MetaProgress.SelectCharacter(idx);
            else MetaProgress.UnlockCharacter(idx);
            RefreshChars();
        }

        private void RefreshShop()
        {
            for (int i = 0; i < _shopRows.Length; i++)
            {
                var u = MetaProgress.Upgrades[i];
                string right = MetaProgress.IsMaxed(i) ? "MAX" : $"{MetaProgress.CostOf(i)}g";
                _shopRows[i].text = $"{u.Name}  Lv{MetaProgress.LevelOf(i)}/{u.MaxLevel}\n<size=20>{u.Desc}  —  {right}</size>";
            }
            RefreshGold();
        }

        private void RefreshChars()
        {
            for (int i = 0; i < _charRows.Length; i++)
            {
                var c = MetaProgress.Characters[i];
                string status;
                if (MetaProgress.SelectedCharacter == i) status = "SELECTED";
                else if (MetaProgress.IsCharacterUnlocked(i)) status = "tap to select";
                else status = $"unlock {c.UnlockCost}g";
                _charRows[i].text = $"{c.Name}  <size=20>({status})</size>\n<size=20>{c.Desc}</size>";
            }
            RefreshGold();
        }

        // ---------------- UI construction ----------------

        private void BuildMain()
        {
            _main = Panel("MainMenuPanel", new Color(0.05f, 0.05f, 0.09f, 0.97f));

            var title = Label(_main.transform, "SURVIVOR.IO", 84, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0f, -220f), new Vector2(900f, 120f));
            title.fontStyle = FontStyle.Bold;
            title.color = new Color(1f, 0.85f, 0.2f);

            _goldMain = Label(_main.transform, "", 34, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0f, -340f), new Vector2(700f, 60f));
            _goldMain.color = new Color(1f, 0.85f, 0.3f);

            Button(_main.transform, "PLAY", new Vector2(0f, 150f), new Vector2(520f, 115f), 54,
                new Color(0.2f, 0.55f, 0.25f), Play);
            Button(_main.transform, "STAGES", new Vector2(0f, 35f), new Vector2(520f, 95f), 38,
                new Color(0.3f, 0.4f, 0.3f), () => { ShowOnly(_stages); RefreshStages(); });
            Button(_main.transform, "CHARACTERS", new Vector2(0f, -75f), new Vector2(520f, 95f), 38,
                new Color(0.2f, 0.3f, 0.5f), () => { ShowOnly(_chars); RefreshChars(); });
            Button(_main.transform, "SHOP", new Vector2(0f, -185f), new Vector2(520f, 95f), 38,
                new Color(0.45f, 0.3f, 0.5f), () => { ShowOnly(_shop); RefreshShop(); });
            Button(_main.transform, "SETTINGS", new Vector2(0f, -295f), new Vector2(520f, 95f), 38,
                new Color(0.35f, 0.35f, 0.4f), () => { ShowOnly(_settings); RefreshSettings(); });
        }

        private void BuildShop()
        {
            _shop = Panel("ShopPanel", new Color(0.05f, 0.05f, 0.09f, 0.98f));
            var t = Label(_shop.transform, "SHOP — upgrades", 50, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(900f, 80f));
            t.fontStyle = FontStyle.Bold; t.color = new Color(1f, 0.85f, 0.3f);

            _goldShop = Label(_shop.transform, "", 32, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0f, -200f), new Vector2(700f, 50f));
            _goldShop.color = new Color(1f, 0.85f, 0.3f);

            float y = -280f;
            for (int i = 0; i < 6; i++)
            {
                int id = i;
                _shopRows[i] = RowButton(_shop.transform, y, new Color(0.16f, 0.14f, 0.22f), () => BuyUpgrade(id));
                y -= 150f;
            }
            Button(_shop.transform, "BACK", new Vector2(0f, 120f), new Vector2(360f, 90f), 38,
                new Color(0.3f, 0.3f, 0.35f), () => ShowOnly(_main), bottom: true);
        }

        private void BuildChars()
        {
            _chars = Panel("CharPanel", new Color(0.05f, 0.05f, 0.09f, 0.98f));
            var t = Label(_chars.transform, "CHARACTERS", 50, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(900f, 80f));
            t.fontStyle = FontStyle.Bold; t.color = new Color(0.8f, 0.9f, 1f);

            _goldChars = Label(_chars.transform, "", 32, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0f, -200f), new Vector2(700f, 50f));
            _goldChars.color = new Color(1f, 0.85f, 0.3f);

            float y = -300f;
            for (int i = 0; i < 3; i++)
            {
                int idx = i;
                _charRows[i] = RowButton(_chars.transform, y, new Color(0.14f, 0.18f, 0.28f), () => PickCharacter(idx));
                y -= 170f;
            }
            Button(_chars.transform, "BACK", new Vector2(0f, 120f), new Vector2(360f, 90f), 38,
                new Color(0.3f, 0.3f, 0.35f), () => ShowOnly(_main), bottom: true);
        }

        private void ShowOnly(GameObject panel)
        {
            _main.SetActive(panel == _main);
            _shop.SetActive(panel == _shop);
            _chars.SetActive(panel == _chars);
            _stages.SetActive(panel == _stages);
            _settings.SetActive(panel == _settings);
        }

        private void BuildSettings()
        {
            _settings = Panel("SettingsPanel", new Color(0.05f, 0.05f, 0.09f, 0.98f));
            var t = Label(_settings.transform, "SETTINGS", 50, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0f, -140f), new Vector2(900f, 80f));
            t.fontStyle = FontStyle.Bold; t.color = new Color(0.85f, 0.85f, 1f);

            _sfxVal = VolumeRow("SFX", -340f, () => AudioManager.Instance?.SfxVolume ?? 0.5f,
                v => AudioManager.Instance?.SetSfxVolume(v));
            _musVal = VolumeRow("Music", -500f, () => AudioManager.Instance?.MusicVolume ?? 0.35f,
                v => AudioManager.Instance?.SetMusicVolume(v));

            Button(_settings.transform, "BACK", new Vector2(0f, 120f), new Vector2(360f, 90f), 38,
                new Color(0.3f, 0.3f, 0.35f), () => ShowOnly(_main), bottom: true);
        }

        private Text VolumeRow(string name, float y, System.Func<float> get, System.Action<float> set)
        {
            Label(_settings.transform, name, 36, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(-180f, y), new Vector2(200f, 70f));

            ButtonTop(_settings.transform, "-", new Vector2(-20f, y), new Vector2(80f, 70f),
                () => { set(Mathf.Clamp01(get() - 0.1f)); RefreshSettings(); });

            var valText = Label(_settings.transform, "", 36, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(90f, y), new Vector2(120f, 70f));

            ButtonTop(_settings.transform, "+", new Vector2(200f, y), new Vector2(80f, 70f),
                () => { set(Mathf.Clamp01(get() + 0.1f)); RefreshSettings(); });
            return valText;
        }

        private void ButtonTop(Transform parent, string text, Vector2 pos, Vector2 size,
            UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("Btn_" + text, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>(); img.color = new Color(0.3f, 0.3f, 0.35f);
            var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);
            var label = Label(go.transform, text, 44, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), Vector2.zero, size);
            label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            label.fontStyle = FontStyle.Bold;
        }

        private void RefreshSettings()
        {
            if (AudioManager.Instance == null) return;
            if (_sfxVal) _sfxVal.text = $"{Mathf.RoundToInt(AudioManager.Instance.SfxVolume * 100)}%";
            if (_musVal) _musVal.text = $"{Mathf.RoundToInt(AudioManager.Instance.MusicVolume * 100)}%";
        }

        private void BuildStages()
        {
            _stages = Panel("StagePanel", new Color(0.05f, 0.05f, 0.09f, 0.98f));
            var t = Label(_stages.transform, "STAGES", 50, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(900f, 80f));
            t.fontStyle = FontStyle.Bold; t.color = new Color(0.85f, 1f, 0.85f);

            _goldStages = Label(_stages.transform, "", 32, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0f, -200f), new Vector2(700f, 50f));
            _goldStages.color = new Color(1f, 0.85f, 0.3f);

            float y = -300f;
            for (int i = 0; i < 3; i++)
            {
                int idx = i;
                _stageRows[i] = RowButton(_stages.transform, y, new Color(0.16f, 0.2f, 0.16f), () => PickStage(idx));
                y -= 170f;
            }
            Button(_stages.transform, "BACK", new Vector2(0f, 120f), new Vector2(360f, 90f), 38,
                new Color(0.3f, 0.3f, 0.35f), () => ShowOnly(_main), bottom: true);
        }

        private void PickStage(int idx)
        {
            if (MetaProgress.IsStageUnlocked(idx)) MetaProgress.SelectStage(idx);
            else MetaProgress.UnlockStage(idx);
            RefreshStages();
        }

        private void RefreshStages()
        {
            for (int i = 0; i < _stageRows.Length; i++)
            {
                var s = MetaProgress.Stages[i];
                string status;
                if (MetaProgress.SelectedStage == i) status = "SELECTED";
                else if (MetaProgress.IsStageUnlocked(i)) status = "tap to select";
                else status = $"unlock {s.UnlockCost}g";
                _stageRows[i].text = $"{s.Name}  <size=20>({status})</size>\n<size=20>{s.Desc}</size>";
            }
            RefreshGold();
        }

        // ---------------- helpers ----------------

        private GameObject Panel(string name, Color bg)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(_canvas.transform, false);
            go.transform.SetAsLastSibling();
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            go.AddComponent<Image>().color = bg;
            return go;
        }

        private Text Label(Transform parent, string text, int size, TextAnchor anchor,
            Vector2 anchorTop, Vector2 pos, Vector2 sizeDelta)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchorTop;
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;
            var t = go.AddComponent<Text>();
            t.font = _font; t.text = text; t.fontSize = size; t.alignment = anchor;
            t.color = Color.white; t.supportRichText = true; t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        private void Button(Transform parent, string text, Vector2 centerPos, Vector2 size, int fontSize,
            Color color, UnityEngine.Events.UnityAction onClick, bool bottom = false)
        {
            var go = new GameObject("Btn_" + text, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, bottom ? 0f : 0.5f);
            rt.pivot = new Vector2(0.5f, bottom ? 0f : 0.5f);
            rt.anchoredPosition = centerPos;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>(); img.color = color;
            var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            var label = Label(go.transform, text, fontSize, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), Vector2.zero, size);
            label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            label.fontStyle = FontStyle.Bold;
        }

        private Text RowButton(Transform parent, float y, Color color, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("Row", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = new Vector2(820f, 135f);
            var img = go.AddComponent<Image>(); img.color = color;
            var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            var label = Label(go.transform, "", 30, TextAnchor.MiddleLeft,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 120f));
            label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            return label;
        }
    }
}
