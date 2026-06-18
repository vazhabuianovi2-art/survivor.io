using System.Collections.Generic;
using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// A short-lived floating damage number. Code-created and pooled — popups are
    /// the highest-churn FX (one per hit), so they are reused instead of being
    /// Instantiated/Destroyed every time.
    /// </summary>
    public class DamagePopup : MonoBehaviour
    {
        private static Font _font;
        private static readonly Queue<DamagePopup> _pool = new Queue<DamagePopup>();

        private TextMesh _text;
        private MeshRenderer _mr;
        private float _life, _maxLife;
        private Vector3 _velocity;
        private Color _baseColor;

        public static DamagePopup Spawn(Vector3 worldPos, float amount, Color? color = null)
        {
            if (_font == null)
                _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            Color c = color ?? new Color(1f, 0.95f, 0.4f);
            Vector3 pos = worldPos + new Vector3(Random.Range(-0.15f, 0.15f), 0.3f, 0f);

            DamagePopup popup = _pool.Count > 0 ? _pool.Dequeue() : Create();
            popup.gameObject.SetActive(true);
            popup.transform.position = pos;
            popup.transform.localScale = Vector3.one;

            popup._text.text  = Mathf.Round(amount).ToString();
            popup._text.color = c;
            popup._baseColor  = c;
            popup._maxLife    = 0.6f;
            popup._life       = popup._maxLife;
            popup._velocity   = new Vector3(0f, 1.6f, 0f);
            return popup;
        }

        private static DamagePopup Create()
        {
            var go = new GameObject("DamagePopup");
            var tm = go.AddComponent<TextMesh>();
            tm.font          = _font;
            tm.fontSize      = 48;
            tm.characterSize = 0.08f;
            tm.anchor        = TextAnchor.MiddleCenter;
            tm.alignment     = TextAlignment.Center;

            var mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterial = _font.material;
            mr.sortingOrder   = 1000;

            var p = go.AddComponent<DamagePopup>();
            p._text = tm;
            p._mr = mr;
            return p;
        }

        private void Update()
        {
            _life -= Time.deltaTime;
            if (_life <= 0f) { Despawn(); return; }

            transform.position += _velocity * Time.deltaTime;
            _velocity.y -= 2.2f * Time.deltaTime;

            float t = 1f - _life / _maxLife;
            transform.localScale = Vector3.one * (1f + 0.3f * Mathf.Clamp01(t * 4f));

            var c = _baseColor;
            c.a = 1f - t * t;
            _text.color = c;
        }

        private void Despawn()
        {
            gameObject.SetActive(false);
            _pool.Enqueue(this);
        }
    }
}
