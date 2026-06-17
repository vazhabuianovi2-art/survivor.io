using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Boss attack patterns layered on top of the base chase. Cycles through a
    /// radial projectile burst, an aimed volley and summoning adds. Below 50% HP
    /// it enters phase 2 and attacks faster.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    [RequireComponent(typeof(Health))]
    public class BossBehavior : MonoBehaviour
    {
        [SerializeField] private float attackInterval = 3f;
        [SerializeField] private float projectileSpeed = 5f;
        [SerializeField] private float projectileDamage = 10f;
        [SerializeField] private GameObject summonPrefab;
        [SerializeField] private int summonCount = 3;

        private Enemy _enemy;
        private Health _health;
        private float _t;
        private int _pattern;

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
            _health = GetComponent<Health>();
            _t = attackInterval;
        }

        private void Update()
        {
            var target = _enemy.Target;
            if (target == null) return;

            bool phase2 = _health.Current < _health.Max * 0.5f;
            float cadence = phase2 ? attackInterval * 0.6f : attackInterval;

            _t -= Time.deltaTime;
            if (_t <= 0f)
            {
                _t = cadence;
                DoAttack(target, phase2);
            }
        }

        private void DoAttack(Transform target, bool phase2)
        {
            switch (_pattern % 3)
            {
                case 0: RadialBurst(phase2 ? 16 : 12); break;
                case 1: AimedVolley(target); break;
                case 2: Summon(); break;
            }
            _pattern++;
        }

        private void RadialBurst(int n)
        {
            for (int i = 0; i < n; i++)
            {
                float a = i * (360f / n) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                EnemyProjectile.Spawn(transform.position, dir, projectileSpeed, projectileDamage, 4f);
            }
            if (CameraFollow.Instance != null) CameraFollow.Instance.Shake(0.2f, 0.15f);
        }

        private void AimedVolley(Transform target)
        {
            Vector2 baseDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            for (int i = -1; i <= 1; i++)
            {
                float r = i * 14f * Mathf.Deg2Rad, c = Mathf.Cos(r), s = Mathf.Sin(r);
                Vector2 dir = new Vector2(baseDir.x * c - baseDir.y * s, baseDir.x * s + baseDir.y * c);
                EnemyProjectile.Spawn(transform.position, dir, projectileSpeed * 1.4f, projectileDamage, 4f);
            }
        }

        private void Summon()
        {
            if (summonPrefab == null) return;
            for (int i = 0; i < summonCount; i++)
            {
                Vector3 off = Random.insideUnitCircle.normalized * 1.5f;
                Instantiate(summonPrefab, transform.position + off, Quaternion.identity);
            }
        }
    }
}
