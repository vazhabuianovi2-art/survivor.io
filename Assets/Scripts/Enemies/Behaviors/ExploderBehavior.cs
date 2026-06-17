using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Rushes the player (default chase) and detonates on contact, dealing
    /// area damage, then dies. Keeps the base Enemy movement.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class ExploderBehavior : MonoBehaviour
    {
        [SerializeField] private float explodeRange = 0.9f;
        [SerializeField] private float blastRadius = 1.9f;
        [SerializeField] private float blastDamage = 22f;

        private Enemy _enemy;
        private bool _done;

        private void Awake() => _enemy = GetComponent<Enemy>();

        private void Update()
        {
            if (_done) return;
            var target = _enemy.Target;
            if (target == null) return;

            if (Vector2.Distance(transform.position, target.position) <= explodeRange)
                Explode(target);
        }

        private void Explode(Transform player)
        {
            _done = true;

            if (Vector2.Distance(transform.position, player.position) <= blastRadius)
            {
                var hp = player.GetComponent<Health>();
                if (hp != null) hp.TakeDamage(blastDamage * PlayerStats.DamageTaken);
            }

            if (CameraFollow.Instance != null) CameraFollow.Instance.Shake(0.25f, 0.18f);

            // Kill self → triggers the Enemy death (drops + smoke FX + despawn).
            // Use exact remaining HP so the damage popup stays a sane number.
            var health = _enemy.HealthComp;
            if (health != null) health.TakeDamage(health.Current);
        }
    }
}
