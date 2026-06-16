using System.Collections.Generic;
using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Spawns glowing orbs that orbit the player and damage enemies they touch.
    /// Levels add orbs, damage and orbit radius. Orb visuals are generated in code.
    /// </summary>
    public class OrbitOrbWeapon : WeaponBase
    {
        private const float AngularSpeed = 130f;   // deg/sec
        private const float HitInterval  = 0.15f;
        private const float OrbHitRadius = 0.38f;

        private readonly List<Transform> _orbs = new List<Transform>();
        private readonly Collider2D[] _buf = new Collider2D[32];
        private float _angle;
        private float _hitTimer;

        public override string DisplayName => "Orbit Orbs";

        public override string DescribeNext()
        {
            int next = Level + 1;
            return next % 2 == 0 ? "+1 orb" : "more orb damage";
        }

        private int OrbCount   => 1 + Level;                       // L1=2 … L5=6
        private float Damage   => (4f + (Level - 1) * 2f) * PlayerStats.Damage;
        private float Radius   => 1.5f + (Level - 1) * 0.12f;

        protected override void OnEquip()    => RebuildOrbs();
        protected override void OnLevelUp()  => RebuildOrbs();

        private void OnDestroy()
        {
            foreach (var o in _orbs) if (o != null) Destroy(o.gameObject);
        }

        private void RebuildOrbs()
        {
            foreach (var o in _orbs) if (o != null) Destroy(o.gameObject);
            _orbs.Clear();

            for (int i = 0; i < OrbCount; i++)
            {
                var go = new GameObject($"Orb{i}");
                go.transform.SetParent(Owner, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = OrbSprite();
                sr.sortingOrder = 20;
                _orbs.Add(go.transform);
            }
        }

        private void Update()
        {
            if (!GameActive) return;

            _angle += AngularSpeed * Time.deltaTime;
            PositionOrbs();

            _hitTimer -= Time.deltaTime;
            if (_hitTimer <= 0f)
            {
                _hitTimer = HitInterval;
                DamageAtOrbs();
            }
        }

        private void PositionOrbs()
        {
            int count = _orbs.Count;
            for (int i = 0; i < count; i++)
            {
                if (_orbs[i] == null) continue;
                float a = (_angle + i * (360f / count)) * Mathf.Deg2Rad;
                _orbs[i].localPosition = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * Radius;
            }
        }

        private void DamageAtOrbs()
        {
            foreach (var orb in _orbs)
            {
                if (orb == null) continue;
                int n = Physics2D.OverlapCircleNonAlloc(orb.position, OrbHitRadius, _buf);
                for (int i = 0; i < n; i++)
                {
                    if (_buf[i] == null || !_buf[i].CompareTag("Enemy")) continue;
                    var hp = _buf[i].GetComponent<Health>();
                    if (hp != null) hp.TakeDamage(Damage);
                }
            }
        }

        // ---- generated orb sprite (cached) ----
        private static Sprite _orbSprite;
        private static Sprite OrbSprite()
        {
            if (_orbSprite != null) return _orbSprite;

            int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float half = size / 2f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half + 0.5f) / half;
                    float dy = (y - half + 0.5f) / half;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float core = Mathf.Clamp01(1f - d / 0.45f);
                    float glow = Mathf.Pow(Mathf.Clamp01(1f - d), 0.7f);
                    float a = Mathf.Max(core, glow * 0.85f);
                    var col = Color.Lerp(new Color(0.6f, 0.4f, 1f), Color.white, core);
                    col.a = a;
                    tex.SetPixel(x, y, col);
                }
            tex.Apply();
            _orbSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return _orbSprite;
        }
    }
}
