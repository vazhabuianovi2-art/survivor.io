using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Auto-fires homing-less bolts at the nearest enemy on a timer. Reuses the
    /// existing Projectile prefab. Levels add damage, fire rate and extra bolts.
    /// </summary>
    public class MagicBoltWeapon : WeaponBase
    {
        private const float Range = 7f;
        private const float Speed = 11f;
        private const float Life  = 1.8f;
        private const float Spread = 14f;

        private float _timer;

        public override string DisplayName => "Magic Bolt";

        public override string DescribeNext()
        {
            int next = Level + 1;
            return (next == 3 || next == 5) ? "+1 bolt & more damage" : "more damage & faster fire";
        }

        private int BoltCount => 1 + (Level >= 3 ? 1 : 0) + (Level >= 5 ? 1 : 0);
        private float Damage   => (6f + (Level - 1) * 4f) * PlayerStats.Damage;
        private float Interval => Mathf.Max(0.3f, 0.9f * Mathf.Pow(0.9f, Level - 1) * PlayerStats.Cooldown);

        private void Update()
        {
            if (!GameActive) return;

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            var target = FindNearestEnemy(Range);
            if (target == null) return;        // fire the instant an enemy is in range

            _timer = Interval;
            Fire(target);
        }

        private void Fire(Transform target)
        {
            var prefab = WeaponManager.Instance != null ? WeaponManager.Instance.ProjectilePrefab : null;
            if (prefab == null) return;

            Vector2 baseDir = ((Vector2)target.position - (Vector2)Owner.position).normalized;
            int count = BoltCount;

            for (int i = 0; i < count; i++)
            {
                float off = count == 1 ? 0f : Mathf.Lerp(-Spread, Spread, i / (float)(count - 1));
                Vector2 dir = Rotate(baseDir, off);
                var go = Instantiate(prefab, Owner.position, Quaternion.identity);
                var p = go.GetComponent<Projectile>();
                if (p != null) p.Launch(dir, Speed, Damage, Life, 0);
            }
        }

        private static Vector2 Rotate(Vector2 v, float deg)
        {
            float r = deg * Mathf.Deg2Rad;
            float c = Mathf.Cos(r), s = Mathf.Sin(r);
            return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
        }
    }
}
