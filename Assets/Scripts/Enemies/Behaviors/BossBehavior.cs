using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Boss attack patterns with telegraphs: each attack flashes a red warning
    /// for a moment before it fires, so the player can react. Cycles radial burst
    /// / aimed volley / summon adds, and speeds up below 50% HP (phase 2).
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    [RequireComponent(typeof(Health))]
    public class BossBehavior : MonoBehaviour
    {
        [SerializeField] private float attackInterval = 3f;
        [SerializeField] private float telegraphTime = 0.6f;
        [SerializeField] private float projectileSpeed = 5f;
        [SerializeField] private float projectileDamage = 10f;
        [SerializeField] private GameObject summonPrefab;
        [SerializeField] private int summonCount = 3;

        private static readonly Color Warn = new Color(1f, 0.2f, 0.2f, 0.35f);

        private Enemy _enemy;
        private Health _health;
        private float _t;
        private int _pattern;

        private bool _pending;
        private float _tele;
        private int _pendingPattern;
        private Vector2 _aimDir = Vector2.right;

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

            if (_pending)
            {
                _tele -= Time.deltaTime;
                if (_tele <= 0f) { Execute(_pendingPattern, phase2); _pending = false; }
                return;
            }

            _t -= Time.deltaTime;
            if (_t <= 0f)
            {
                _t = phase2 ? attackInterval * 0.6f : attackInterval;
                Begin(target);
            }
        }

        private void Begin(Transform target)
        {
            _pendingPattern = _pattern % 3;
            _pattern++;
            _aimDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            _pending = true;
            _tele = telegraphTime;
            Telegraph(_pendingPattern);
        }

        private void Telegraph(int pattern)
        {
            Vector3 pos = transform.position;
            switch (pattern)
            {
                case 0: // radial → warning ring around the boss
                    TempFx.Spawn(pos, FxSprites.SoftDisc(), Warn, Vector3.one * 7f, telegraphTime, 1.1f);
                    break;
                case 1: // aimed → warning line toward the player
                    float ang = Mathf.Atan2(_aimDir.y, _aimDir.x) * Mathf.Rad2Deg;
                    TempFx.Spawn(pos + (Vector3)(_aimDir * 2.2f), FxSprites.SoftDisc(), Warn,
                        new Vector3(4.5f, 0.6f, 1f), telegraphTime, 1f, ang);
                    break;
                default: // summon → warning ring where adds will appear
                    TempFx.Spawn(pos, FxSprites.SoftDisc(), new Color(1f, 0.5f, 0.1f, 0.35f),
                        Vector3.one * 3.4f, telegraphTime, 1.1f);
                    break;
            }
        }

        private void Execute(int pattern, bool phase2)
        {
            switch (pattern)
            {
                case 0: RadialBurst(phase2 ? 16 : 12); break;
                case 1: AimedVolley(); break;
                default: Summon(); break;
            }
        }

        private void RadialBurst(int n)
        {
            for (int i = 0; i < n; i++)
            {
                float a = i * (360f / n) * Mathf.Deg2Rad;
                EnemyProjectile.Spawn(transform.position, new Vector2(Mathf.Cos(a), Mathf.Sin(a)),
                    projectileSpeed, projectileDamage, 4f);
            }
            if (CameraFollow.Instance != null) CameraFollow.Instance.Shake(0.2f, 0.15f);
        }

        private void AimedVolley()
        {
            for (int i = -1; i <= 1; i++)
            {
                float r = i * 14f * Mathf.Deg2Rad, c = Mathf.Cos(r), s = Mathf.Sin(r);
                Vector2 dir = new Vector2(_aimDir.x * c - _aimDir.y * s, _aimDir.x * s + _aimDir.y * c);
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
