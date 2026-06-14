using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Survivor-style auto weapon: on a fixed interval it finds the nearest enemy
    /// within range and fires a projectile at it. No aiming required from the
    /// player — this is the core of the genre.
    /// </summary>
    public class AutoAttackWeapon : MonoBehaviour
    {
        [Header("Projectile")]
        [SerializeField] private GameObject projectilePrefab;
        [Tooltip("Where projectiles spawn from. Defaults to this transform.")]
        [SerializeField] private Transform firePoint;

        [Header("Stats")]
        [SerializeField] private float fireInterval = 0.4f;
        [SerializeField] private float range = 7f;
        [SerializeField] private float projectileSpeed = 12f;
        [SerializeField] private float damage = 1f;
        [SerializeField] private float projectileLifetime = 2f;
        [SerializeField] private int pierce = 0;

        private float _timer;
        // Reused buffer so targeting allocates nothing per shot.
        private readonly Collider2D[] _hits = new Collider2D[128];

        private void Awake()
        {
            if (firePoint == null) firePoint = transform;
        }

        private void Update()
        {
            if (projectilePrefab == null) return;

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            Transform target = FindNearestEnemy();
            if (target == null) return; // keep timer ready so we fire as soon as one appears

            Fire(target);
            _timer = fireInterval;
        }

        private Transform FindNearestEnemy()
        {
            int count = Physics2D.OverlapCircleNonAlloc(firePoint.position, range, _hits);
            Transform nearest = null;
            float bestSqr = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var col = _hits[i];
                if (col == null || !col.CompareTag("Enemy")) continue;

                float sqr = ((Vector2)col.transform.position - (Vector2)firePoint.position).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    nearest = col.transform;
                }
            }
            return nearest;
        }

        private void Fire(Transform target)
        {
            Vector2 dir = (target.position - firePoint.position).normalized;
            var go = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            var proj = go.GetComponent<Projectile>();
            if (proj != null)
                proj.Launch(dir, projectileSpeed, damage, projectileLifetime, pierce);
        }
    }
}
