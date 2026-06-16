using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Fires a fast, fully-piercing bolt at the nearest enemy — a "laser lance"
    /// that punches through a whole line of enemies. Levels add damage and rate.
    /// </summary>
    public class EnergyLaserWeapon : WeaponBase
    {
        private const float Range = 9f;
        private const float Speed = 20f;
        private const float Life  = 0.9f;
        private const int Pierce  = 99;

        private float _timer;

        public override string DisplayName => "Energy Laser";
        public override string DescribeNext() => "more damage & faster fire";

        private float Damage   => (8f + (Level - 1) * 5f) * PlayerStats.Damage;
        private float Interval => Mathf.Max(0.55f, 1.6f * Mathf.Pow(0.9f, Level - 1) * PlayerStats.Cooldown);

        private void Update()
        {
            if (!GameActive) return;
            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            var target = FindNearestEnemy(Range);
            if (target == null) return;

            _timer = Interval;
            var prefab = WeaponManager.Instance != null ? WeaponManager.Instance.ProjectilePrefab : null;
            if (prefab == null) return;

            Vector2 dir = ((Vector2)target.position - (Vector2)Owner.position).normalized;
            var go = Instantiate(prefab, Owner.position, Quaternion.identity);
            go.transform.localScale = new Vector3(1.6f, 0.7f, 1f);   // stretched, beam-like
            var p = go.GetComponent<Projectile>();
            if (p != null) p.Launch(dir, Speed, Damage, Life, Pierce);
        }
    }
}
