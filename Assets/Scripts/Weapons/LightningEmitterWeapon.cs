using UnityEngine;

namespace SurvivorIO
{
    /// <summary>Periodically zaps several enemies in range with instant lightning.</summary>
    public class LightningEmitterWeapon : WeaponBase
    {
        private const float Range = 7f;
        private readonly Collider2D[] _buf = new Collider2D[64];
        private float _t;

        public override string DisplayName => "Lightning";
        public override string DescribeNext()
        {
            int n = Level + 1;
            return (n == 3 || n == 5) ? "+1 strike & more damage" : "more damage & faster";
        }

        private int Strikes  => 2 + (Level >= 3 ? 1 : 0) + (Level >= 5 ? 1 : 0);   // 2..4
        private float Damage => (10f + (Level - 1) * 6f) * PlayerStats.Damage;
        private float Interval => Mathf.Max(0.45f, 1.3f * Mathf.Pow(0.9f, Level - 1) * PlayerStats.Cooldown);

        private void Update()
        {
            if (Owner == null || !GameActive) return;
            _t -= Time.deltaTime;
            if (_t > 0f) return;

            int n = Physics2D.OverlapCircleNonAlloc(Owner.position, Range, _buf);
            int struck = 0;
            float dmg = Damage;
            for (int i = 0; i < n && struck < Strikes; i++)
            {
                if (_buf[i] == null || !_buf[i].CompareTag("Enemy")) continue;
                var hp = _buf[i].GetComponent<Health>();
                if (hp == null) continue;
                hp.TakeDamage(dmg);
                Strike(_buf[i].transform.position);
                struck++;
            }
            if (struck > 0) _t = Interval;   // only reset cooldown if it actually fired
        }

        private void Strike(Vector3 pos)
        {
            // thin bright vertical bolt flash
            TempFx.Spawn(pos + Vector3.up * 0.4f, FxSprites.SoftDisc(),
                new Color(0.7f, 0.9f, 1f, 0.9f), new Vector3(0.18f, 1.6f, 1f), 0.18f, 1.1f);
        }
    }
}
