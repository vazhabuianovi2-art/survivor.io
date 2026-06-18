using UnityEngine;

namespace SurvivorIO
{
    /// <summary>A brief sprite that fades (and optionally grows) then destroys itself.
    /// Used for one-shot effects like lightning bolts and explosions.</summary>
    public class TempFx : MonoBehaviour
    {
        private SpriteRenderer _sr;
        private float _life, _maxLife, _growTo;
        private Vector3 _baseScale;
        private Color _color;

        public static TempFx Spawn(Vector3 pos, Sprite sprite, Color color, Vector3 scale,
            float life = 0.25f, float growTo = 1f, float rotationZ = 0f)
        {
            var go = new GameObject("TempFx");
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 50;
            sr.color = color;

            var fx = go.AddComponent<TempFx>();
            fx._sr = sr;
            fx._color = color;
            fx._maxLife = fx._life = life;
            fx._baseScale = scale;
            fx._growTo = growTo;
            return fx;
        }

        private void Update()
        {
            _life -= Time.deltaTime;
            if (_life <= 0f) { Destroy(gameObject); return; }

            float t = 1f - _life / _maxLife;            // 0 → 1
            transform.localScale = _baseScale * Mathf.Lerp(1f, _growTo, t);
            var c = _color;
            c.a = _color.a * (1f - t);
            _sr.color = c;
        }
    }
}
