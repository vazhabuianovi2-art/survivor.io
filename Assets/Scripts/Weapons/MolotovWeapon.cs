using UnityEngine;

namespace SurvivorIO
{
    /// <summary>Lobs a flask at the nearest enemy that leaves a burning zone.</summary>
    public class MolotovWeapon : WeaponBase
    {
        private const float Range = 7f;
        private const float ZoneLife = 3f;
        private float _t;

        public override string DisplayName => "Molotov";
        public override string DescribeNext()
        {
            int n = Level + 1;
            return (n % 2 == 0) ? "bigger fire & more damage" : "more damage & faster";
        }

        private float Radius     => 1.4f + (Level - 1) * 0.2f;
        private float DmgPerTick  => (4f + (Level - 1) * 2.5f) * PlayerStats.Damage;
        private float Interval    => Mathf.Max(1f, 2.6f * Mathf.Pow(0.92f, Level - 1) * PlayerStats.Cooldown);

        private void Update()
        {
            if (!GameActive) return;
            _t -= Time.deltaTime;
            if (_t > 0f) return;

            var target = FindNearestEnemy(Range);
            if (target == null) return;
            _t = Interval;

            Vector3 pos = target.position;
            FireZone.Spawn(pos, Radius, DmgPerTick, ZoneLife);
            TempFx.Spawn(pos, FxSprites.SoftDisc(), new Color(1f, 0.6f, 0.2f, 0.8f),
                Vector3.one * (Radius * 1.2f), 0.3f, 1.4f);
        }
    }
}
