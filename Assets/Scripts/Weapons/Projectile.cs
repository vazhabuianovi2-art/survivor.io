using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// A straight-flying projectile. Travels along its launch direction, damages
    /// enemies it overlaps, and despawns after a lifetime or once it has hit the
    /// allowed number of targets.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        private Vector2 _direction;
        private float _speed;
        private float _damage;
        private float _lifeRemaining;
        private int _hitsRemaining;

        /// <summary>
        /// Configure a freshly spawned projectile.
        /// </summary>
        /// <param name="pierce">How many extra enemies it passes through (0 = single hit).</param>
        public void Launch(Vector2 direction, float speed, float damage, float lifetime, int pierce)
        {
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            _lifeRemaining = lifetime;
            _hitsRemaining = pierce + 1;

            // Orient the sprite along the travel direction.
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void Awake()
        {
            var rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Update()
        {
            transform.position += (Vector3)(_direction * (_speed * Time.deltaTime));

            _lifeRemaining -= Time.deltaTime;
            if (_lifeRemaining <= 0f)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Enemy")) return;

            var health = other.GetComponent<Health>();
            if (health == null) return;

            health.TakeDamage(_damage);

            _hitsRemaining--;
            if (_hitsRemaining <= 0)
                Destroy(gameObject);
        }
    }
}
