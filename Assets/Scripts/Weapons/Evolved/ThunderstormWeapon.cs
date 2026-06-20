using UnityEngine;

namespace SurvivorIO
{
    /// <summary>Evolved Lightning: rapidly zaps every enemy in a large radius.</summary>
    public class ThunderstormWeapon : WeaponBase
    {
        public override int MaxLevel => 1;
        public override bool IsEvolved => true;
        public override string DisplayName => "Thunderstorm";
        public override string DescribeNext() => "Evolved";

        private const float Range = 8.5f;
        private const float Interval = 0.5f;
        private readonly Collider2D[] _buf = new Collider2D[128];
        private float _t;

        private void Update()
        {
            if (Owner == null || !GameActive) return;
            _t -= Time.deltaTime;
            if (_t > 0f) return;
            _t = Interval;

            float dmg = 26f * PlayerStats.Damage;
            int n = Physics2D.OverlapCircleNonAlloc(Owner.position, Range, _buf);
            int flashes = 0;
            for (int i = 0; i < n; i++)
            {
                if (_buf[i] == null || !_buf[i].CompareTag("Enemy")) continue;
                _buf[i].GetComponent<Health>()?.TakeDamage(dmg);
                if (flashes < 8)   // cap visuals so a big crowd doesn't spam FX
                {
                    TempFx.Spawn(_buf[i].transform.position + Vector3.up * 0.4f, FxSprites.SoftDisc(),
                        new Color(0.7f, 0.9f, 1f, 0.9f), new Vector3(0.18f, 1.6f, 1f), 0.16f, 1.1f);
                    flashes++;
                }
            }
            if (CameraFollow.Instance != null) CameraFollow.Instance.Shake(0.12f, 0.1f);
        }
    }
}
