using UnityEngine;

namespace SurvivorIO
{
    /// <summary>Evolved Boomerang: hurls a full ring of returning chakrams.</summary>
    public class ChakramStormWeapon : WeaponBase
    {
        public override int MaxLevel => 1;
        public override bool IsEvolved => true;
        public override string DisplayName => "Chakram Storm";
        public override string DescribeNext() => "Evolved";

        private const int Count = 8;
        private const float Speed = 10f;
        private const float MaxDist = 4.5f;
        private const float Interval = 1.3f;
        private float _t;

        private void Update()
        {
            if (!GameActive) return;
            _t -= Time.deltaTime;
            if (_t > 0f) return;
            _t = Interval;

            float dmg = 16f * PlayerStats.Damage;
            for (int i = 0; i < Count; i++)
            {
                float a = i * (360f / Count) * Mathf.Deg2Rad;
                Boomerang.Spawn(Owner, new Vector2(Mathf.Cos(a), Mathf.Sin(a)), Speed, dmg, MaxDist);
            }
        }
    }
}
