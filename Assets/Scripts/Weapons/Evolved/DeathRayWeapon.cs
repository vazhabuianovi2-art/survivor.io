using UnityEngine;

namespace SurvivorIO
{
    /// <summary>Evolved Energy Laser: a very rapid, fully-piercing high-damage beam.</summary>
    public class DeathRayWeapon : WeaponBase
    {
        public override int MaxLevel => 1;
        public override bool IsEvolved => true;
        public override string DisplayName => "Death Ray";
        public override string DescribeNext() => "Evolved";

        private const float Range = 11f, Speed = 26f, Life = 1f, Interval = 0.3f;
        private float _t;

        private void Update()
        {
            if (!GameActive) return;
            _t -= Time.deltaTime;
            if (_t > 0f) return;
            var target = FindNearestEnemy(Range);
            if (target == null) return;
            _t = Interval;

            var prefab = WeaponManager.Instance != null ? WeaponManager.Instance.ProjectilePrefab : null;
            if (prefab == null) return;

            Vector2 dir = ((Vector2)target.position - (Vector2)Owner.position).normalized;
            var go = Object.Instantiate(prefab, Owner.position, Quaternion.identity);
            go.transform.localScale = new Vector3(2.2f, 0.8f, 1f);
            var p = go.GetComponent<Projectile>();
            if (p != null) p.Launch(dir, Speed, 30f * PlayerStats.Damage, Life, 99);
        }
    }
}
