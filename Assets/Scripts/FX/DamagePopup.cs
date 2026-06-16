using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// A short-lived floating damage number. Created entirely in code via
    /// <see cref="Spawn"/> — no prefab needed. Rises and fades, then destroys.
    /// </summary>
    public class DamagePopup : MonoBehaviour
    {
        private static Font _font;

        private TextMesh _text;
        private float _life;
        private float _maxLife;
        private Vector3 _velocity;
        private Color _baseColor;

        public static DamagePopup Spawn(Vector3 worldPos, float amount, Color? color = null)
        {
            if (_font == null)
                _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var go = new GameObject("DamagePopup");
            go.transform.position = worldPos + new Vector3(Random.Range(-0.15f, 0.15f), 0.3f, 0f);

            var tm = go.AddComponent<TextMesh>();
            tm.text          = Mathf.Round(amount).ToString();
            tm.font          = _font;
            tm.fontSize      = 48;
            tm.characterSize = 0.08f;
            tm.anchor        = TextAnchor.MiddleCenter;
            tm.alignment     = TextAlignment.Center;
            tm.color         = color ?? new Color(1f, 0.95f, 0.4f);

            var mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterial = _font.material;     // built-in font atlas material
            mr.sortingOrder   = 1000;               // above everything

            var popup = go.AddComponent<DamagePopup>();
            popup.Init(tm, color ?? new Color(1f, 0.95f, 0.4f));
            return popup;
        }

        private void Init(TextMesh tm, Color baseColor)
        {
            _text      = tm;
            _baseColor = baseColor;
            _maxLife   = 0.6f;
            _life      = _maxLife;
            _velocity  = new Vector3(0f, 1.6f, 0f);
        }

        private void Update()
        {
            _life -= Time.deltaTime;
            if (_life <= 0f) { Destroy(gameObject); return; }

            transform.position += _velocity * Time.deltaTime;
            _velocity.y -= 2.2f * Time.deltaTime;          // slight deceleration

            float t = 1f - _life / _maxLife;               // 0 → 1 over lifetime
            float scale = 1f + 0.3f * Mathf.Clamp01(t * 4f); // quick pop-in
            transform.localScale = Vector3.one * scale;

            var c = _baseColor;
            c.a = 1f - t * t;                              // fade out toward the end
            _text.color = c;
        }
    }
}
