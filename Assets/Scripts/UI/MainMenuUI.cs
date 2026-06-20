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

        private GameObject _main, _shop, _chars, _stages, _settings, _gear;
        private Text _goldMain, _goldShop, _goldChars, _goldStages, _goldGear;
        private Text _sfxVal, _musVal, _stageCard;
        private readonly Text[] _shopRows = new Text[6];
        private readonly Text[] _charRows = new Text[3];
        private readonly Text[] _stageRows = new Text[3];
        private readonly Text[] _gearRows = new Text[8];

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
            BuildGear();

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
            _gear.SetActive(false);
            Time.timeScale = 1f;
        }

        private void RefreshGold()
        {
            string g = $"Gold: {MetaProgress.Gold}";
            if (_goldMain) _goldMain.text = g;
            if (_goldShop) _goldShop.text = g;
            if (_goldChars) _goldChars.text = g;
            if (_goldStages) _goldStages.text = g;
            if (_goldGear) _goldGear.text = g;
        }

        private void BuildGear()
        {
            _gear = Panel("GearPanel", new Color(0.05f, 0.05f, 0.09f, 0.98f));
            var t = Label(_gear.transform, "GEAR", 50, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0f, -90f), new Vector2(900f, 70f));
            t.fontStyle = FontStyle.Bold; t.color = new Color(1f, 0.85f, 0.4f);

            _goldGear = Label(_gear.transform, "", 30, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(700f, 44f));
            _goldGear.color = new Color(1f, 0.85f, 0.3f);

            float y = -220f;
            for (int i = 0; i < GearSystem.SlotCount; i++)
            {
                int slot = i;
                var row = RowButton(_gear.transform, y, new Color(0.15f, 0.15f, 0.2f), () => UpgradeSlot(slot));
                row.rectTransform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(820f, 78f);
                row.fontSize = 26;
                _gearRows[i] = row;
                y -= 90f;
            }

            Button(_gear.transform, "OPEN CHEST  (100g)", new Vector2(0f, 230f), new Vector2(520f, 95f), 36,
                new Color(0.5f, 0.4f, 0.2f), OpenChest, bottom: true);
            Button(_gear.transform, "BACK", new Vector2(0f, 120f), new Vector2(360f, 85f), 36,
                new Color(0.3f, 0.3f, 0.35f), () => ShowOnly(_main), bottom: true);
        }

        private void OpenChest()
        {
            var item = GearSystem.RollChest();
            if (item != null && AudioManager.Instance != null) AudioManager.Instance.PlayLevelUp();
            RefreshGear();
        }

        private void UpgradeSlot(int slot)
        {
            var g = GearSystem.EquippedIn(slot);
            if (g != null) GearSystem.Upgrade(g);
            RefreshGear();
        }

        private void RefreshGear()
        {
            for (int i = 0; i < _gearRows.Length; i++)
            {
                var g = GearSystem.EquippedIn(i);
                if (g == null)
                {
                    _gearRows[i].text = $"{GearSystem.SlotNames[i]}: <size=20>empty</size>";
                    _gearRows[i].color = new Color(0.6f, 0.6f, 0.6f);
                }
                else
                {
                    string rar = GearSystem.RarityNames[g.rarity];
                    _gearRows[i].text =
                        $"{GearSystem.SlotNames[i]}: {rar} Lv{g.level}  <size=20>{GearSystem.StatLabel(g)}  ·  up {GearSystem.UpgradeCost(g)}g</size>";
                    _gearRows[i].color = GearSystem.RarityColors[g.rarity];
                }
            }
            RefreshGold();
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
            _main = Panel("MainMenuPanel", Color.white);
            var bg = _main.GetComponent<Image>();
            bg.sprite = GradientSprite();
            bg.type = Image.Type.Simple;
            bg.color = Color.white;

            // Top resource bar
            var barBg = ChildImage(_main.transform, new Color(0f, 0f, 0f, 0.35f),
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -6f), new Vector2(-20f, 80f));
            _goldMain = Label(barBg.transform, "", 34, TextAnchor.MiddleRight,
                new Vector2(1f, 0.5f), new Vector2(-30f, 0f), new Vector2(420f, 60f));
            _goldMain.rectTransform.pivot = new Vector2(1f, 0.5f);
            _goldMain.color = new Color(1f, 0.92f, 0.5f);

            var title = Label(_main.transform, "SURVIVOR.IO", 64, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(28f, -10f), new Vector2(560f, 70f));
            title.rectTransform.pivot = new Vector2(0f, 1f);
            title.fontStyle = FontStyle.Bold;
            title.color = new Color(1f, 0.9f, 0.45f);

            // Central stage card
            var card = ChildImage(_main.transform, new Color(0f, 0f, 0f, 0.4f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 170f), new Vector2(680f, 420f));
            _stageCard = Label(card.transform, "", 30, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 380f));
            _stageCard.color = Color.white;

            // Big START button
            Button(_main.transform, "START", new Vector2(0f, -210f), new Vector2(560f, 150f), 64,
                new Color(0.25f, 0.7f, 0.3f), Play);

            // Bottom navigation row
            BottomNav("GEAR",    -380f, new Color(0.5f, 0.42f, 0.2f),  () => { ShowOnly(_gear); RefreshGear(); });
            BottomNav("SHOP",    -190f, new Color(0.5f, 0.32f, 0.55f), () => { ShowOnly(_shop); RefreshShop(); });
            BottomNav("HEROES",   0f,   new Color(0.25f, 0.35f, 0.6f), () => { ShowOnly(_chars); RefreshChars(); });
            BottomNav("STAGE",    190f, new Color(0.3f, 0.45f, 0.32f), () => { ShowOnly(_stages); RefreshStages(); });
            BottomNav("OPTIONS",  380f, new Color(0.4f, 0.4f, 0.45f),  () => { ShowOnly(_settings); RefreshSettings(); });
        }

        private void BottomNav(string text, float x, Color color, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("Nav_" + text, typeof(RectTransform));
            go.transform.SetParent(_main.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(x, 40f);
            rt.sizeDelta = new Vector2(178f, 120f);
            var img = go.AddComponent<Image>(); img.color = color;
            var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);
            var label = Label(go.transform, text, 28, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(178f, 120f));
            label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            label.fontStyle = FontStyle.Bold;
        }

        private void RefreshMain()
        {
            if (_stageCard != null)
            {
                var s = MetaProgress.Stages[MetaProgress.SelectedStage];
                var ch = MetaProgress.Characters[MetaProgress.SelectedCharacter];
                _stageCard.text =
                    $"<size=46><b>{MetaProgress.SelectedStage + 1}. {s.Name}</b></size>\n" +
                    $"<size=24>{s.Desc}</size>\n\n" +
                    $"<size=30>Hero:  {ch.Name}</size>\n" +
                    $"<size=22>tap START to battle</size>";
            }
            RefreshGold();
        }

        private GameObject ChildImage(Transform parent, Color color, Vector2 aMin, Vector2 aMax,
            Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var go = new GameObject("Img", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = pivot;
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            go.AddComponent<Image>().color = color;
            return go;
        }

        private static Sprite _gradient;
        private static Sprite GradientSprite()
        {
            if (_gradient != null) return _gradient;
            int h = 256;
            var tex = new Texture2D(1, h, TextureFormat.RGBA32, false);
            var top = new Color(0.16f, 0.08f, 0.26f);   // deep purple
            var mid = new Color(0.42f, 0.16f, 0.45f);   // magenta
            var bot = new Color(0.95f, 0.5f, 0.22f);    // warm orange
            for (int y = 0; y < h; y++)
            {
                float t = y / (float)(h - 1);
                Color c = t < 0.5f ? Color.Lerp(bot, mid, t * 2f) : Color.Lerp(mid, top, (t - 0.5f) * 2f);
                tex.SetPixel(0, y, c);
            }
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();
            _gradient = Sprite.Create(tex, new Rect(0, 0, 1, h), new Vector2(0.5f, 0.5f), 100f);
            return _gradient;
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
            _gear.SetActive(panel == _gear);
            if (panel == _main) RefreshMain();
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
