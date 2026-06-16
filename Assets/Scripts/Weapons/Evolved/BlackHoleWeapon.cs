using UnityEngine;

namespace SurvivorIO
{
    /// <summary>Evolved Forcefield: a large aura that pulls enemies inward and
    /// deals heavy damage to everything caught in it.</summary>
    public class BlackHoleWeapon : WeaponBase
    {
        public override int MaxLevel => 1;
        public override bool IsEvolved => true;
        public override string DisplayName => "Black Hole";
        public override string DescribeNext() => "Evolved";

        private const float Radius = 3.2f, PullSpeed = 2.2f, TickInterval = 0.3f, DamagePerTick = 8f;

        private readonly Collider2D[] _buf = new Collider2D[96];
        private Transform _ring;
        private float _tick;

        protected override void OnEquip()
        {
            var go = new GameObject("BlackHoleRing");
            go.transform.SetParent(Owner, false);
            go.transform.localPosition = Vector3.zero;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = FxSprites.SoftDisc();
            sr.sortingOrder = -5;
            sr.color = new Color(0.4f, 0.2f, 0.6f, 0.4f);
            go.transform.localScale = Vector3.one * (Radius * 2f);
            _ring = go.transform;
        }

        private void Update()
        {
            if (!GameActive) return;

            // Continuous pull every frame.
            Vector2 center = Owner.position;
            int pn = Physics2D.OverlapCircleNonAlloc(center, Radius, _buf);
            for (int i = 0; i < pn; i++)
            {
                if (_buf[i] == null || !_buf[i].CompareTag("Enemy")) continue;
                var t = _buf[i].transform;
                t.position = Vector2.MoveTowards(t.position, center, PullSpeed * Time.deltaTime);
            }

            if (_ring != null)
                _ring.Rotate(0f, 0f, 60f * Time.deltaTime);

            _tick -= Time.deltaTime;
            if (_tick > 0f) return;
            _tick = TickInterval;

            float dmg = DamagePerTick * PlayerStats.Damage;
            int n = Physics2D.OverlapCircleNonAlloc(center, Radius, _buf);
            for (int i = 0; i < n; i++)
            {
                if (_buf[i] == null || !_buf[i].CompareTag("Enemy")) continue;
                var hp = _buf[i].GetComponent<Health>();
                if (hp != null) hp.TakeDamage(dmg);
            }
        }
    }
}
