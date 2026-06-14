using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Applies enemy contact damage to the player's <see cref="Health"/> with a
    /// short invulnerability window between hits, and ends the run on death.
    /// Uses an overlap check in FixedUpdate (rather than collision callbacks) so
    /// it works even when the player's body is asleep, and naturally pauses with
    /// the timescale during level-up / game-over.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class PlayerHealthController : MonoBehaviour
    {
        [Tooltip("Seconds of invulnerability after taking a hit.")]
        [SerializeField] private float invulnTime = 0.5f;
        [Tooltip("Radius around the player that counts as enemy contact.")]
        [SerializeField] private float contactRadius = 0.7f;

        private Health _health;
        private float _invuln;
        private readonly Collider2D[] _buffer = new Collider2D[16];

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void OnEnable() => _health.Died += OnDied;
        private void OnDisable() => _health.Died -= OnDied;

        private void FixedUpdate()
        {
            if (_health.IsDead) return;

            if (_invuln > 0f)
            {
                _invuln -= Time.fixedDeltaTime;
                return;
            }

            int n = Physics2D.OverlapCircleNonAlloc(transform.position, contactRadius, _buffer);
            float damage = 0f;
            for (int i = 0; i < n; i++)
            {
                var enemy = _buffer[i].GetComponent<Enemy>();
                if (enemy != null) damage = Mathf.Max(damage, enemy.ContactDamage);
            }

            if (damage > 0f)
            {
                _health.TakeDamage(damage);
                _invuln = invulnTime;
            }
        }

        private void OnDied(Health h)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.TriggerGameOver();
        }
    }
}
