using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Keeps its distance from the player (kites) and fires projectiles on a timer.
    /// Takes over movement from the base Enemy chase.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class RangedBehavior : MonoBehaviour
    {
        [SerializeField] private float preferredMin = 4f;
        [SerializeField] private float preferredMax = 6.5f;
        [SerializeField] private float moveSpeed = 2.2f;
        [SerializeField] private float fireInterval = 2f;
        [SerializeField] private float projectileSpeed = 5.5f;
        [SerializeField] private float projectileDamage = 6f;

        private Enemy _enemy;
        private Rigidbody2D _rb;
        private float _t;

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
            _enemy.CedeMovement();
            _rb = GetComponent<Rigidbody2D>();
            _t = fireInterval * 0.5f;
        }

        private void FixedUpdate()
        {
            var target = _enemy.Target;
            if (target == null) { _rb.linearVelocity = Vector2.zero; return; }

            Vector2 toP = (Vector2)target.position - _rb.position;
            float dist = toP.magnitude;
            Vector2 dirP = dist > 0.001f ? toP / dist : Vector2.zero;

            if (dist < preferredMin)       _rb.linearVelocity = -dirP * moveSpeed;   // back away
            else if (dist > preferredMax)  _rb.linearVelocity = dirP * moveSpeed;     // close in
            else                           _rb.linearVelocity = Vector2.zero;         // hold & fire
        }

        private void Update()
        {
            var target = _enemy.Target;
            if (target == null) return;

            _t -= Time.deltaTime;
            if (_t > 0f) return;
            _t = fireInterval;

            Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            EnemyProjectile.Spawn(transform.position, dir, projectileSpeed, projectileDamage, 4f);
        }
    }
}
