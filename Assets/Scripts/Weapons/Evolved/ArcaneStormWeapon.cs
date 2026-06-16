using UnityEngine;

namespace SurvivorIO
{
    /// <summary>Evolved Magic Bolt: a rapid spread-barrage of high-damage bolts.</summary>
    public class ArcaneStormWeapon : WeaponBase
    {
        public override int MaxLevel => 1;
        public override bool IsEvolved => true;
        public override string DisplayName => "Arcane Storm";
        public override string DescribeNext() => "Evolved";

        private const float Range = 9f, Speed = 14f, Life = 2f, Interval = 0.3f;
        private const int Bolts = 5;
        private const float Spread = 40f;
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

            float dmg = 22f * PlayerStats.Damage;
            Vector2 baseDir = ((Vector2)target.position - (Vector2)Owner.position).normalized;
            for (int i = 0; i < Bolts; i++)
            {
                float off = Mathf.Lerp(-Spread * 0.5f, Spread * 0.5f, i / (float)(Bolts - 1));
                float r = off * Mathf.Deg2Rad, c = Mathf.Cos(r), s = Mathf.Sin(r);
                Vector2 dir = new Vector2(baseDir.x * c - baseDir.y * s, baseDir.x * s + baseDir.y * c);
                var go = Object.Instantiate(prefab, Owner.position, Quaternion.identity);
                var p = go.GetComponent<Projectile>();
                if (p != null) p.Launch(dir, Speed, dmg, Life, 1);
            }
        }
    }
}
