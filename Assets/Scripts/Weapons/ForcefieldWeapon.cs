using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// A persistent damaging aura around the player. Ticks damage to every enemy
    /// inside its radius. Levels add radius and damage. Visual is generated in code.
    /// </summary>
    public class ForcefieldWeapon : WeaponBase
    {
        private const float TickInterval = 0.4f;

        private readonly Collider2D[] _buf = new Collider2D[64];
        private Transform _ring;
        private float _tick;

        public override string DisplayName => "Forcefield";
        public override string DescribeNext() => "+radius & more damage";

        private float Radius => 1.4f + (Level - 1) * 0.28f;
        private float Damage => (3f + (Level - 1) * 2f) * PlayerStats.Damage;

        protected override void OnEquip()   => BuildOrUpdateRing();
        protected override void OnLevelUp() => BuildOrUpdateRing();

        private void OnDestroy()
        {
            if (_ring != null) Destroy(_ring.gameObject);
        }

        private void BuildOrUpdateRing()
        {
            if (_ring == null)
            {
                var go = new GameObject("ForcefieldRing");
                go.transform.SetParent(Owner, false);
                go.transform.localPosition = Vector3.zero;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = RingSprite();
                sr.sortingOrder = -5;                 // behind the player
                sr.color = new Color(0.55f, 0.4f, 1f, 0.32f);
                _ring = go.transform;
            }
            // sprite is 1 world-unit diameter at 100ppu (64px) → scale = diameter
            _ring.localScale = Vector3.one * (Radius * 2f);
        }

        private void Update()
        {
            if (Owner == null || !GameActive) return;
            _tick -= Time.deltaTime;
            if (_tick > 0f) return;
            _tick = TickInterval;

            int n = Physics2D.OverlapCircleNonAlloc(Owner.position, Radius, _buf);
            for (int i = 0; i < n; i++)
            {
                if (_buf[i] == null || !_buf[i].CompareTag("Enemy")) continue;
                var hp = _buf[i].GetComponent<Health>();
                if (hp != null) hp.TakeDamage(Damage);
            }
        }

        private static Sprite _ringSprite;
        private static Sprite RingSprite()
        {
            if (_ringSprite != null) return _ringSprite;

            int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float half = size / 2f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half + 0.5f) / half;
                    float dy = (y - half + 0.5f) / half;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    // soft filled disc, slightly brighter at the rim
                    float fill = Mathf.Clamp01(1f - d);
                    float rim = Mathf.Clamp01(1f - Mathf.Abs(d - 0.92f) / 0.12f);
                    float a = Mathf.Clamp01(fill * 0.5f + rim * 0.6f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            tex.Apply();
            _ringSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return _ringSprite;
        }
    }
}
