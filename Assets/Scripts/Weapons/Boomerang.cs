using System.Collections.Generic;
using UnityEngine;

namespace SurvivorIO
{
    /// <summary>A projectile that flies out to a max distance then returns to the
    /// player, damaging each enemy it passes once per leg.</summary>
    public class Boomerang : MonoBehaviour
    {
        private Transform _owner;
        private Vector2 _dir;
        private float _speed, _damage, _maxDist, _traveled;
        private bool _returning;
        private readonly Collider2D[] _buf = new Collider2D[24];
        private readonly HashSet<Health> _hit = new HashSet<Health>();

        public static void Spawn(Transform owner, Vector2 dir, float speed, float damage, float maxDist)
        {
            var go = new GameObject("Boomerang");
            go.transform.position = owner.position;
            go.transform.localScale = Vector3.one * 0.5f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = FxSprites.Tinted(new Color(0.5f, 1f, 0.9f));
            sr.sortingOrder = 12;

            var b = go.AddComponent<Boomerang>();
            b._owner = owner;
            b._dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
            b._speed = speed;
            b._damage = damage;
            b._maxDist = maxDist;
        }

        private void Update()
        {
            transform.Rotate(0f, 0f, 720f * Time.deltaTime);

            float step = _speed * Time.deltaTime;
            if (!_returning)
            {
                transform.position += (Vector3)(_dir * step);
                _traveled += step;
                if (_traveled >= _maxDist) { _returning = true; _hit.Clear(); }
            }
            else
            {
                if (_owner == null) { Destroy(gameObject); return; }
                transform.position = Vector2.MoveTowards(transform.position, _owner.position, step);
                if (Vector2.Distance(transform.position, _owner.position) < 0.4f) { Destroy(gameObject); return; }
            }

            int n = Physics2D.OverlapCircleNonAlloc(transform.position, 0.5f, _buf);
            for (int i = 0; i < n; i++)
            {
                if (_buf[i] == null || !_buf[i].CompareTag("Enemy")) continue;
                var hp = _buf[i].GetComponent<Health>();
                if (hp != null && !_hit.Contains(hp)) { hp.TakeDamage(_damage); _hit.Add(hp); }
            }
        }
    }
}
