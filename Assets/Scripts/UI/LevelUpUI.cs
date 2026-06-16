using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SurvivorIO
{
    /// <summary>
    /// Shows the level-up skill-choice screen. Builds its own UI at runtime (a
    /// dark overlay + three skill cards), pauses the game while open, and applies
    /// the chosen upgrade. Handles several queued level-ups in sequence.
    /// </summary>
    public class LevelUpUI : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;

        private PlayerExperience _xp;
        private WeaponManager _weapons;
        private PassiveManager _passives;
        private GameObject _panel;
        private readonly Button[] _cardButtons = new Button[3];
        private readonly Text[] _cardTitles = new Text[3];
        private readonly Text[] _cardDescs = new Text[3];

        private readonly List<Skill> _allSkills = new List<Skill>();
        private readonly List<Skill> _current = new List<Skill>();
        private int _pending;
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
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            _xp = player.GetComponent<PlayerExperience>();
            _weapons = FindFirstObjectByType<WeaponManager>();
            _passives = FindFirstObjectByType<PassiveManager>();
            BuildSkillList(player);
            if (_xp != null) _xp.LeveledUp += OnLeveledUp;
        }

        private void OnDestroy()
        {
            if (_xp != null) _xp.LeveledUp -= OnLeveledUp;
        }

        private void OnLeveledUp(int level)
        {
            _pending++;
            if (!_panel.activeSelf) ShowChoices();
        }

        private void ShowChoices()
        {
            _current.Clear();

            // Static skills (sword upgrades + universal) plus dynamic weapon
            // choices from the WeaponManager (acquire / level up auto-weapons).
            var pool = new List<Skill>(_allSkills);
            if (_weapons != null)
                pool.AddRange(_weapons.BuildWeaponSkills());
            if (_passives != null)
                pool.AddRange(_passives.BuildPassiveSkills());

            for (int i = 0; i < 3 && pool.Count > 0; i++)
            {
                int idx = Random.Range(0, pool.Count);
                _current.Add(pool[idx]);
                pool.RemoveAt(idx);
            }

            for (int i = 0; i < 3; i++)
            {
                bool has = i < _current.Count;
                _cardButtons[i].gameObject.SetActive(has);
                if (has)
                {
                    _cardTitles[i].text = _current[i].Name;
                    _cardDescs[i].text = _current[i].Description;

                    // Highlight evolution offers in gold.
                    bool evolve = _current[i].Name.StartsWith("★");
                    if (_cardButtons[i].targetGraphic is Image img)
                        img.color = evolve
                            ? new Color(0.42f, 0.34f, 0.10f, 1f)
                            : new Color(0.16f, 0.18f, 0.26f, 1f);
                    _cardTitles[i].color = evolve
                        ? new Color(1f, 0.8f, 0.15f)
                        : new Color(1f, 0.85f, 0.3f);
                }
            }

            _panel.SetActive(true);
            Time.timeScale = 0f;
        }

        private void Choose(int index)
        {
            if (index < _current.Count)
                _current[index].Apply?.Invoke();

            _pending--;
            if (_pending > 0)
            {
                ShowChoices();
            }
            else
            {
                _panel.SetActive(false);
                Time.timeScale = 1f;
            }
        }

        private void BuildSkillList(GameObject player)
        {
            var ranged = player.GetComponent<AutoAttackWeapon>();
            var melee  = player.GetComponent<MeleeWeapon>();

            // Melee weapon skills (when MeleeWeapon is present and enabled)
            if (melee != null && melee.enabled)
            {
                _allSkills.Add(new Skill("Sword Mastery",   "+25% sword damage",   () => melee.UpgradeDamage(1.25f)));
                _allSkills.Add(new Skill("Wider Slash",     "+20% swing range",    () => melee.UpgradeRange(1.2f)));
                _allSkills.Add(new Skill("Swift Blade",     "+20% attack speed",   () => melee.UpgradeCooldown(0.8f)));
                _allSkills.Add(new Skill("Blood Rush",      "+30% sword damage",   () => melee.UpgradeDamage(1.3f)));
                _allSkills.Add(new Skill("Berserker",       "+25% swing range",    () => melee.UpgradeRange(1.25f)));
            }
            else if (ranged != null)
            {
                _allSkills.Add(new Skill("Sharp Rounds", "+1 projectile damage",   () => ranged.UpgradeDamage(1f)));
                _allSkills.Add(new Skill("Rapid Fire",   "+25% attack speed",      () => ranged.UpgradeFireRate(0.8f)));
                _allSkills.Add(new Skill("Long Shot",    "+2 attack range",         () => ranged.UpgradeRange(2f)));
                _allSkills.Add(new Skill("Piercing",     "Projectiles pierce +1",  () => ranged.UpgradePierce(1)));
                _allSkills.Add(new Skill("Multi Shot",   "+1 projectile per volley",() => ranged.UpgradeProjectileCount(1)));
            }

            // Movement / health are now covered by the Swift Boots / Vitality
            // passive items (see PassiveManager), so no universal skills here.
        }

        // ---------- UI construction ----------

        private void BuildUI()
        {
            _panel = CreateChild("LevelUpPanel", canvas.transform);
            var panelRt = _panel.GetComponent<RectTransform>();
            Stretch(panelRt);
            var bg = _panel.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.8f);

            // Title
            var title = CreateText("Title", _panel.transform, "LEVEL UP!", 64, TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            title.color = new Color(1f, 0.85f, 0.2f);
            var titleRt = title.rectTransform;
            titleRt.anchorMin = new Vector2(0.5f, 1f);
            titleRt.anchorMax = new Vector2(0.5f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -180f);
            titleRt.sizeDelta = new Vector2(800f, 100f);

            // Three cards, centered horizontally (sized to fit a portrait screen).
            const float cardW = 280f, cardH = 440f, gap = 30f;
            float startX = -(cardW + gap);
            for (int i = 0; i < 3; i++)
            {
                var card = CreateChild($"Card{i}", _panel.transform);
                var rt = card.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(cardW, cardH);
                rt.anchoredPosition = new Vector2(startX + i * (cardW + gap), -20f);

                var img = card.AddComponent<Image>();
                img.color = new Color(0.16f, 0.18f, 0.26f, 1f);
                var btn = card.AddComponent<Button>();
                btn.targetGraphic = img;
                var colors = btn.colors;
                colors.highlightedColor = new Color(0.28f, 0.32f, 0.45f);
                colors.pressedColor = new Color(0.4f, 0.45f, 0.6f);
                btn.colors = colors;

                int captured = i;
                btn.onClick.AddListener(() => Choose(captured));

                var name = CreateText("Name", card.transform, "", 38, TextAnchor.MiddleCenter);
                name.fontStyle = FontStyle.Bold;
                name.color = new Color(1f, 0.85f, 0.3f);
                var nameRt = name.rectTransform;
                nameRt.anchorMin = new Vector2(0f, 1f);
                nameRt.anchorMax = new Vector2(1f, 1f);
                nameRt.pivot = new Vector2(0.5f, 1f);
                nameRt.anchoredPosition = new Vector2(0f, -40f);
                nameRt.sizeDelta = new Vector2(-30f, 90f);

                var desc = CreateText("Desc", card.transform, "", 28, TextAnchor.UpperCenter);
                desc.color = Color.white;
                var descRt = desc.rectTransform;
                descRt.anchorMin = new Vector2(0f, 0f);
                descRt.anchorMax = new Vector2(1f, 1f);
                descRt.pivot = new Vector2(0.5f, 0.5f);
                descRt.offsetMin = new Vector2(20f, 30f);
                descRt.offsetMax = new Vector2(-20f, -150f);

                _cardButtons[i] = btn;
                _cardTitles[i] = name;
                _cardDescs[i] = desc;
            }
        }

        private GameObject CreateChild(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
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

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
