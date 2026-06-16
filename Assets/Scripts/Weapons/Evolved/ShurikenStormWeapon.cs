using UnityEngine;

namespace SurvivorIO
{
    /// <summary>Evolved Kunai: fires a full 360° ring of knives on a fast timer.</summary>
    public class ShurikenStormWeapon : WeaponBase
    {
        public override int MaxLevel => 1;
        public override bool IsEvolved => true;
        public override string DisplayName => "Shuriken Storm";
        public override string DescribeNext() => "Evolved";

        private const int Count = 14;
        private const float Speed = 9f, Life = 1.4f, Interval = 0.55f;
        private float _t;
        private float _spin;

        private void Update()
        {
            if (!GameActive) return;
            _t -= Time.deltaTime;
            if (_t > 0f) return;
            _t = Interval;
            _spin += 13f;   // rotate the ring a bit each volley

            var prefab = WeaponManager.Instance != null ? WeaponManager.Instance.ProjectilePrefab : null;
            if (prefab == null) return;

            float dmg = 10f * PlayerStats.Damage;
            for (int i = 0; i < Count; i++)
            {
                float a = (_spin + i * (360f / Count)) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                var go = Object.Instantiate(prefab, Owner.position, Quaternion.identity);
                var p = go.GetComponent<Projectile>();
                if (p != null) p.Launch(dir, Speed, dmg, Life, 0);
            }
        }
    }
}
