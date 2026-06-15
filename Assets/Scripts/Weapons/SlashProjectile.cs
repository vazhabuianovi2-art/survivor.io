using System.Collections.Generic;
using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// A crescent slash that travels forward a fixed distance, damaging any enemy
    /// it overlaps along the way, then destroys itself.
    /// </summary>
    public class SlashProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 8f;
        [SerializeField] private float maxDistance = 2.5f;
        [SerializeField] private float hitRadius = 0.5f;

        private float _damage;
        private float _traveled;
        private readonly Collider2D[] _buf = new Collider2D[16];
        private readonly HashSet<Health> _hit = new HashSet<Health>();

        public void Init(float damage, Vector2 direction)
        {
            _damage = damage;
            transform.up = direction;       // rotate slash to face travel dir
        }

        private void Update()
        {
            float step = speed * Time.deltaTime;
            transform.position += (Vector3)(Vector2)transform.up * step;
            _traveled += step;

            // Damage enemies along path
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
