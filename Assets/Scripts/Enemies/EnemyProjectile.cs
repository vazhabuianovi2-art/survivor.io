using UnityEngine;

namespace SurvivorIO
{
    /// <summary>A projectile fired BY an enemy that damages the player. Code-spawned.</summary>
    public class EnemyProjectile : MonoBehaviour
    {
        private Vector2 _dir;
        private float _speed, _dmg, _life;
        private const float HitRadius = 0.4f;
        private Transform _player;

        public static void Spawn(Vector3 pos, Vector2 dir, float speed, float dmg, float life)
        {
            var go = new GameObject("EnemyProjectile");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = FxSprites.Tinted(new Color(1f, 0.3f, 0.2f));
            sr.sortingOrder = 9;
            go.transform.localScale = Vector3.one * 0.4f;

            var p = go.AddComponent<EnemyProjectile>();
            p._dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
            p._speed = speed;
            p._dmg = dmg;
            p._life = life;
        }

        private void Start()
        {
            var pl = GameObject.FindGameObjectWithTag("Player");
            if (pl != null) _player = pl.transform;
        }

        private void Update()
        {
            transform.position += (Vector3)(_dir * (_speed * Time.deltaTime));
            _life -= Time.deltaTime;
            if (_life <= 0f) { Destroy(gameObject); return; }

            if (_player != null && Vector2.Distance(transform.position, _player.position) <= HitRadius)
            {
                var hp = _player.GetComponent<Health>();
                if (hp != null) hp.TakeDamage(_dmg * PlayerStats.DamageTaken);
                Destroy(gameObject);
            }
        }
    }
}
