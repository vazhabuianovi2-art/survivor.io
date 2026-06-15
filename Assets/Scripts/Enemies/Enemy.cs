using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Basic melee enemy: continuously steers toward the player via Rigidbody2D
    /// and removes itself when its <see cref="Health"/> is depleted.
    /// Stage 4 will hook the death event to drop an XP gem; stage 5 will use
    /// <see cref="contactDamage"/> against the player's health.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Health))]
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float contactDamage = 8f;
        [SerializeField] private SpriteRenderer spriteToFlip;
        [Tooltip("Rigged-character visual root flipped via localScale.x (preferred over spriteToFlip).")]
        [SerializeField] private Transform visualToFlip;

        [Header("Drops")]
        [SerializeField] private GameObject xpGemPrefab;
        [SerializeField] private int xpGemCount = 1;

        [Header("Death FX")]
        [SerializeField] private GameObject deathFxPrefab;

        [Header("Scaling (set by WaveManager at runtime)")]
        [SerializeField] private float healthMultiplier = 1f;
        [SerializeField] private float damageMultiplier = 1f;
        [SerializeField] private float speedMultiplier = 1f;

        /// <summary>Damage this enemy deals to the player on contact (used in stage 5).</summary>
        public float ContactDamage => contactDamage;

        /// <summary>Number of enemies currently alive — used by the spawner to cap counts.</summary>
        public static int AliveCount { get; private set; }

        /// <summary>Total enemies killed this run (drives the kill counter UI in stage 5).</summary>
        public static int KillCount { get; private set; }

        /// <summary>Reset run-scoped counters (call when a new run starts).</summary>
        public static void ResetCounters()
        {
            AliveCount = 0;
            KillCount = 0;
        }

        private Rigidbody2D _rb;
        private Health _health;
        private Transform _target;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _health = GetComponent<Health>();
            if (spriteToFlip == null)
                spriteToFlip = GetComponentInChildren<SpriteRenderer>();

            // Apply stat multipliers
            if (healthMultiplier != 1f)
                _health.SetMaxHealth(_health.Max * healthMultiplier);
            moveSpeed *= speedMultiplier;
            contactDamage *= damageMultiplier;
        }

        private void OnEnable()
        {
            AliveCount++;
            _health.Died += HandleDied;
            AcquireTarget();
        }

        private void OnDisable()
        {
            AliveCount--;
            _health.Died -= HandleDied;
        }

        private void AcquireTarget()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) _target = player.transform;
        }

        private void FixedUpdate()
        {
            if (_target == null)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 dir = (Vector2)_target.position - _rb.position;
            if (dir.sqrMagnitude > 0.0001f) dir.Normalize();
            _rb.linearVelocity = dir * moveSpeed;

            if (Mathf.Abs(dir.x) > 0.01f)
            {
                if (visualToFlip != null)
                {
                    var s = visualToFlip.localScale;
                    // Rig faces +x by default; face left when moving left.
                    s.x = Mathf.Abs(s.x) * (dir.x < 0f ? -1f : 1f);
                    visualToFlip.localScale = s;
                }
                else if (spriteToFlip != null)
                {
                    spriteToFlip.flipX = dir.x < 0f;
                }
            }
        }

        private void HandleDied(Health h)
        {
            KillCount++;
            if (xpGemPrefab != null)
            {
                for (int i = 0; i < xpGemCount; i++)
                {
                    var offset = Random.insideUnitCircle * 0.3f;
                    Instantiate(xpGemPrefab, transform.position + (Vector3)offset, Quaternion.identity);
                }
            }
            if (deathFxPrefab != null)
                Instantiate(deathFxPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
