using UnityEngine;

namespace SurvivorIO
{
    /// <summary>Throws returning boomerangs at the nearest enemy.</summary>
    public class BoomerangWeapon : WeaponBase
    {
        private const float Range = 7f;
        private const float Speed = 9f;
        private const float MaxDist = 4f;
        private float _t;

        public override string DisplayName => "Boomerang";
        public override string DescribeNext()
        {
            int n = Level + 1;
            return (n == 3 || n == 5) ? "+1 boomerang & more damage" : "more damage & faster";
        }

        private int Count    => 1 + (Level >= 3 ? 1 : 0) + (Level >= 5 ? 1 : 0);   // 1..3
        private float Damage  => (8f + (Level - 1) * 4f) * PlayerStats.Damage;
        private float Interval => Mathf.Max(0.6f, 1.6f * Mathf.Pow(0.92f, Level - 1) * PlayerStats.Cooldown);

        private void Update()
        {
            if (!GameActive) return;
            _t -= Time.deltaTime;
            if (_t > 0f) return;

            var target = FindNearestEnemy(Range);
            if (target == null) return;
            _t = Interval;

            Vector2 baseDir = ((Vector2)target.position - (Vector2)Owner.position).normalized;
            int c = Count;
            for (int i = 0; i < c; i++)
            {
                float off = c == 1 ? 0f : Mathf.Lerp(-25f, 25f, i / (float)(c - 1));
                float r = off * Mathf.Deg2Rad, cs = Mathf.Cos(r), sn = Mathf.Sin(r);
                Vector2 dir = new Vector2(baseDir.x * cs - baseDir.y * sn, baseDir.x * sn + baseDir.y * cs);
                Boomerang.Spawn(Owner, dir, Speed, Damage, MaxDist);
            }
        }
    }
}
