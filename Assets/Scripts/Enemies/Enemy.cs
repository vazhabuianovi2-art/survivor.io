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
        [Tooltip("Elite / boss: drop a reward chest on death instead of the usual loot.")]
        [SerializeField] private bool dropsChest = false;

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
        private bool _moveCeded;

        // Exposed so optional behavior components (Charger/Ranged/…) can drive movement.
        public Transform Target => _target;
        public Rigidbody2D Body => _rb;
        public Health HealthComp => _health;
        public float BaseMoveSpeed => moveSpeed;
        public float ContactDamageValue => contactDamage;

        /// <summary>Called by a behavior component to take over movement from the default chase.</summary>
        public void CedeMovement() => _moveCeded = true;

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

            if (!_moveCeded)
            {
                Vector2 dir = (Vector2)_target.position - _rb.position;
                if (dir.sqrMagnitude > 0.0001f) dir.Normalize();
                _rb.linearVelocity = dir * moveSpeed;
            }

            // Flip to face travel direction (works for ceded behaviors too).
            Flip(_rb.linearVelocity.x);
        }

        /// <summary>Flip the visual to face the given horizontal velocity.</summary>
        public void Flip(float vx)
        {
            if (Mathf.Abs(vx) <= 0.01f) return;
            if (visualToFlip != null)
            {
                var s = visualToFlip.localScale;
                s.x = Mathf.Abs(s.x) * (vx < 0f ? -1f : 1f);
                visualToFlip.localScale = s;
            }
            else if (spriteToFlip != null)
            {
                spriteToFlip.flipX = vx < 0f;
            }
        }

        private void DropLoot()
        {
            if (dropsChest)
            {
                Pickup.Spawn(PickupType.Chest, transform.position);
                return;
            }

            Vector3 Jitter() => transform.position + (Vector3)(Random.insideUnitCircle * 0.3f);

            // Most enemies drop a little gold.
            if (Random.value < 0.5f)
                Pickup.Spawn(PickupType.Gold, Jitter(), Random.Range(1, 4));

            // Rare consumables.
            float r = Random.value;
            if (r < 0.02f)      Pickup.Spawn(PickupType.Magnet, Jitter());
            else if (r < 0.04f) Pickup.Spawn(PickupType.Bomb, Jitter());
            else if (r < 0.07f) Pickup.Spawn(PickupType.Heal, Jitter());
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
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayDeath();

            DropLoot();
            Destroy(gameObject);
        }
    }
}
