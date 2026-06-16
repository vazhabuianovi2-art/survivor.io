using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Throws a wide fan of kunai toward the nearest enemy — a shotgun-style
    /// burst. Levels add knives and damage. Reuses the Projectile prefab.
    /// </summary>
    public class KunaiWeapon : WeaponBase
    {
        private const float Range = 5.5f;
        private const float Speed = 10f;
        private const float Life  = 1.1f;
        private const float TotalSpread = 50f;

        private float _timer;

        public override string DisplayName => "Kunai";
        public override string DescribeNext() => "+1 knife & more damage";

        private int Knives     => 3 + (Level - 1);                       // 3 … 7
        private float Damage   => (4f + (Level - 1) * 2f) * PlayerStats.Damage;
        private float Interval => Mathf.Max(0.5f, 1.3f * Mathf.Pow(0.92f, Level - 1) * PlayerStats.Cooldown);

        private void Update()
        {
            if (!GameActive) return;
            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            var target = FindNearestEnemy(Range);
            if (target == null) return;

            _timer = Interval;
            Fire(((Vector2)target.position - (Vector2)Owner.position).normalized);
        }

        private void Fire(Vector2 baseDir)
        {
            var prefab = WeaponManager.Instance != null ? WeaponManager.Instance.ProjectilePrefab : null;
            if (prefab == null) return;

            int n = Knives;
            for (int i = 0; i < n; i++)
            {
                float off = n == 1 ? 0f : Mathf.Lerp(-TotalSpread * 0.5f, TotalSpread * 0.5f, i / (float)(n - 1));
                float r = off * Mathf.Deg2Rad;
                float c = Mathf.Cos(r), s = Mathf.Sin(r);
                Vector2 dir = new Vector2(baseDir.x * c - baseDir.y * s, baseDir.x * s + baseDir.y * c);

                var go = Instantiate(prefab, Owner.position, Quaternion.identity);
                var p = go.GetComponent<Projectile>();
                if (p != null) p.Launch(dir, Speed, Damage, Life, 0);
            }
        }
    }
}
