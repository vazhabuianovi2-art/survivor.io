using UnityEngine;

namespace SurvivorIO
{
    /// <summary>Evolved Molotov: frequent large fire zones at the nearest enemy plus
    /// burning ground around the player.</summary>
    public class InfernoWeapon : WeaponBase
    {
        public override int MaxLevel => 1;
        public override bool IsEvolved => true;
        public override string DisplayName => "Inferno";
        public override string DescribeNext() => "Evolved";

        private const float Range = 8f;
        private const float Interval = 1.2f;
        private float _t;

        private void Update()
        {
            if (Owner == null || !GameActive) return;
            _t -= Time.deltaTime;
            if (_t > 0f) return;
            _t = Interval;

            float dmg = 10f * PlayerStats.Damage;

            // burning ground under the player (self-defense)
            FireZone.Spawn(Owner.position, 2.2f, dmg, 1.6f);

            // big blaze at the nearest enemy
            var target = FindNearestEnemy(Range);
            if (target != null)
            {
                FireZone.Spawn(target.position, 3f, dmg * 1.4f, 2.5f);
                TempFx.Spawn(target.position, FxSprites.SoftDisc(), new Color(1f, 0.6f, 0.2f, 0.8f),
                    Vector3.one * 3.4f, 0.3f, 1.4f);
            }
        }
    }
}
