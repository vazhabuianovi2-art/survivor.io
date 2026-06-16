using System.Collections.Generic;
using UnityEngine;

namespace SurvivorIO
{
    public class SlashProjectile : MonoBehaviour
    {
        [SerializeField] private float speed       = 8f;
        [SerializeField] private float maxDistance = 2.5f;
        [SerializeField] private float hitRadius   = 0.5f;
        [Tooltip("Degrees added on top of the travel angle so the crescent's convex (cutting) edge leads the direction of travel.")]
        [SerializeField] private float facingOffsetDeg = -45f;

        private float _damage;
        private float _traveled;
        private Vector2 _dir = Vector2.right;
        private readonly Collider2D[] _buf = new Collider2D[16];
        private readonly HashSet<Health> _hit = new HashSet<Health>();

        private SpriteRenderer _sr;

        private void Awake()
        {
            _sr = GetComponentInChildren<SpriteRenderer>();
        }

        public void Init(float damage, Vector2 direction)
        {
            _damage = damage;
            _dir    = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;

            // Orient so the crescent's convex (cutting) edge leads travel direction.
            float angle = Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg + facingOffsetDeg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void Update()
        {
            float step = speed * Time.deltaTime;
            transform.position += (Vector3)(_dir * step);
            _traveled += step;

            // Fade out only in the last 30% of travel
            if (_sr != null)
            {
                float t = Mathf.Clamp01((_traveled / maxDistance - 0.7f) / 0.3f);
                var c = _sr.color;
                c.a = Mathf.Lerp(1f, 0f, t * t);
                _sr.color = c;
            }

            // Damage enemies along path (each enemy hit only once per slash)
            int n = Physics2D.OverlapCircleNonAlloc(transform.position, hitRadius, _buf);
            for (int i = 0; i < n; i++)
            {
                if (!_buf[i].CompareTag("Enemy")) continue;
                var hp = _buf[i].GetComponent<Health>();
                if (hp != null && !_hit.Contains(hp))
                {
                    hp.TakeDamage(_damage);
                    _hit.Add(hp);
                }
            }

            if (_traveled >= maxDistance)
                Destroy(gameObject);
        }
    }
}
