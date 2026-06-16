using System.Collections.Generic;
using UnityEngine;

namespace SurvivorIO
{
    /// <summary>Evolved Orbit Orbs: many large, fast orbs on a wide ring, high damage.</summary>
    public class SaturnRingWeapon : WeaponBase
    {
        public override int MaxLevel => 1;
        public override bool IsEvolved => true;
        public override string DisplayName => "Saturn Ring";
        public override string DescribeNext() => "Evolved";

        private const int OrbCount = 7;
        private const float Radius = 2.4f, AngularSpeed = 180f, HitInterval = 0.12f, HitRadius = 0.5f;

        private readonly List<Transform> _orbs = new List<Transform>();
        private readonly Collider2D[] _buf = new Collider2D[48];
        private float _angle, _hitTimer;

        protected override void OnEquip()
        {
            for (int i = 0; i < OrbCount; i++)
            {
                var go = new GameObject($"SaturnOrb{i}");
                go.transform.SetParent(Owner, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = FxSprites.Orb();
                sr.sortingOrder = 20;
                go.transform.localScale = Vector3.one * 1.6f;
                _orbs.Add(go.transform);
            }
        }

        private void Update()
        {
            if (!GameActive) return;
            _angle += AngularSpeed * Time.deltaTime;
            for (int i = 0; i < _orbs.Count; i++)
            {
                float a = (_angle + i * (360f / _orbs.Count)) * Mathf.Deg2Rad;
                _orbs[i].localPosition = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * Radius;
            }

            _hitTimer -= Time.deltaTime;
            if (_hitTimer > 0f) return;
            _hitTimer = HitInterval;

            float dmg = 12f * PlayerStats.Damage;
            foreach (var orb in _orbs)
            {
                int n = Physics2D.OverlapCircleNonAlloc(orb.position, HitRadius, _buf);
                for (int i = 0; i < n; i++)
                {
                    if (_buf[i] == null || !_buf[i].CompareTag("Enemy")) continue;
                    var hp = _buf[i].GetComponent<Health>();
                    if (hp != null) hp.TakeDamage(dmg);
                }
            }
        }
    }
}
