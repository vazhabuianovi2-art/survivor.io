using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Renders a green HP bar and a yellow cooldown bar in world-space below the player.
    /// </summary>
    public class PlayerBars : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private MeleeWeapon meleeWeapon;

        [SerializeField] private float barWidth   = 0.55f;
        [SerializeField] private float barHeight  = 0.07f;
        [SerializeField] private float yOffsetHP  = -0.52f;
        [SerializeField] private float yOffsetCD  = -0.64f;

        private Transform _hpFill;
        private Transform _cdFill;
        private Transform _hpBg;
        private Transform _cdBg;

        private static Sprite _whiteSpr;

        private void Awake()
        {
            _hpBg  = MakeBar("HPBg",   new Color(0.1f, 0.1f, 0.1f, 0.8f), 19, yOffsetHP);
            _hpFill = MakeFill("HPFill", new Color(0.15f, 0.95f, 0.25f, 1f), 20, _hpBg);

            _cdBg  = MakeBar("CDBg",   new Color(0.1f, 0.1f, 0.1f, 0.8f), 19, yOffsetCD);
            _cdFill = MakeFill("CDFill", new Color(1f, 0.85f, 0f, 1f), 20, _cdBg);
        }

        private void LateUpdate()
        {
            // Keep bars upright regardless of player rotation
            if (_hpBg)  _hpBg.rotation  = Quaternion.identity;
            if (_cdBg)  _cdBg.rotation  = Quaternion.identity;

            UpdateFill(_hpFill, health != null ? Mathf.Clamp01(health.Current / health.Max) : 1f);
            float cdRatio = meleeWeapon != null ? meleeWeapon.CooldownProgress : 0f;
            UpdateFill(_cdFill, cdRatio);
        }

        private static void UpdateFill(Transform fill, float ratio)
        {
            if (fill == null) return;
            fill.localScale    = new Vector3(ratio, 1f, 1f);
            fill.localPosition = new Vector3(-0.5f + ratio * 0.5f, 0f, -0.01f);
        }

        // ── helpers ─────────────────────────────────────────────────────────

        private Transform MakeBar(string name, Color color, int order, float yOffset)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, yOffset, 0f);
            go.transform.localScale    = new Vector3(barWidth, barHeight, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = WhiteSprite();
            sr.color        = color;
            sr.sortingOrder = order;
            return go.transform;
        }

        private Transform MakeFill(string name, Color color, int order, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(-0.5f, 0f, -0.01f);
            go.transform.localScale    = Vector3.one;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = WhiteSprite();
            sr.color        = color;
            sr.sortingOrder = order;
            return go.transform;
        }

        private static Sprite WhiteSprite()
        {
            if (_whiteSpr != null) return _whiteSpr;
            var tex = new Texture2D(4, 4);
            var px  = new Color[16];
            for (int i = 0; i < 16; i++) px[i] = Color.white;
            tex.SetPixels(px);
            tex.Apply();
            _whiteSpr = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            return _whiteSpr;
        }
    }
}
