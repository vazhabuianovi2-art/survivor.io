using System.Collections.Generic;
using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Survivor-style auto weapon: on a fixed interval it finds the nearest
    /// enemies within range and fires a projectile at each. No aiming required —
    /// this is the core of the genre. Level-up skills tune its stats via the
    /// public Upgrade* methods.
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
        [Tooltip("How many of the nearest enemies are targeted each volley.")]
        [SerializeField] private int projectileCount = 1;

        private float _timer;
        // Reused buffers so targeting allocates nothing per shot.
        private readonly Collider2D[] _hits = new Collider2D[256];
        private readonly List<Transform> _targets = new List<Transform>(8);

        private void Awake()
        {
            if (firePoint == null) firePoint = transform;
        }

        private void Update()
        {
            if (projectilePrefab == null) return;

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            FindNearestEnemies(projectileCount, _targets);
            if (_targets.Count == 0) return; // ready to fire the instant one appears

            foreach (var t in _targets)
                Fire(t);
            _timer = fireInterval;
        }

        /// <summary>Fills <paramref name="results"/> with up to <paramref name="count"/> nearest enemies.</summary>
        private void FindNearestEnemies(int count, List<Transform> results)
        {
            results.Clear();
            int n = Physics2D.OverlapCircleNonAlloc(firePoint.position, range, _hits);
            if (n == 0) return;

            // Selection: repeatedly pick the closest not-yet-chosen enemy.
            Vector2 origin = firePoint.position;
            var chosen = new HashSet<int>();
            for (int picked = 0; picked < count; picked++)
            {
                int bestIdx = -1;
                float bestSqr = float.MaxValue;
                for (int i = 0; i < n; i++)
                {
                    if (chosen.Contains(i)) continue;
                    var col = _hits[i];
                    if (col == null || !col.CompareTag("Enemy")) continue;

                    float sqr = ((Vector2)col.transform.position - origin).sqrMagnitude;
                    if (sqr < bestSqr) { bestSqr = sqr; bestIdx = i; }
                }
                if (bestIdx < 0) break;
                chosen.Add(bestIdx);
                results.Add(_hits[bestIdx].transform);
            }
        }

        private void Fire(Transform target)
        {
            Vector2 dir = (target.position - firePoint.position).normalized;
            var go = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            var proj = go.GetComponent<Projectile>();
            if (proj != null)
                proj.Launch(dir, projectileSpeed, damage, projectileLifetime, pierce);
        }

        // ----- Upgrade hooks used by level-up skills -----
        public void UpgradeDamage(float amount) => damage += amount;
        public void UpgradeFireRate(float multiplier) => fireInterval = Mathf.Max(0.05f, fireInterval * multiplier);
        public void UpgradeRange(float amount) => range += amount;
        public void UpgradePierce(int amount) => pierce += amount;
        public void UpgradeProjectileCount(int amount) => projectileCount += amount;
    }
}
