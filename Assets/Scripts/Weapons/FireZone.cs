using UnityEngine;

namespace SurvivorIO
{
    /// <summary>A burning area that ticks damage to enemies inside it for a while.</summary>
    public class FireZone : MonoBehaviour
    {
        private const float TickInterval = 0.35f;
        private readonly Collider2D[] _buf = new Collider2D[48];
        private float _radius, _dmgPerTick, _life, _tick;

        public static FireZone Spawn(Vector3 pos, float radius, float dmgPerTick, float life)
        {
            var go = new GameObject("FireZone");
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = FxSprites.SoftDisc();
            sr.sortingOrder = 2;
            sr.color = new Color(1f, 0.45f, 0.15f, 0.4f);
            go.transform.localScale = Vector3.one * (radius * 2f);

            var z = go.AddComponent<FireZone>();
            z._radius = radius;
            z._dmgPerTick = dmgPerTick;
            z._life = life;
            return z;
        }

        private void Update()
        {
            _life -= Time.deltaTime;
            if (_life <= 0f) { Destroy(gameObject); return; }

            _tick -= Time.deltaTime;
            if (_tick > 0f) return;
            _tick = TickInterval;

            int n = Physics2D.OverlapCircleNonAlloc(transform.position, _radius, _buf);
            for (int i = 0; i < n; i++)
            {
                if (_buf[i] == null || !_buf[i].CompareTag("Enemy")) continue;
                _buf[i].GetComponent<Health>()?.TakeDamage(_dmgPerTick);
            }
        }
    }
}
