using System.Collections.Generic;
using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Melee sword weapon. When present on the Player it disables the ranged
    /// AutoAttackWeapon and instead swings the sword pivot every <see cref="cooldown"/>
    /// seconds whenever an enemy is within <see cref="range"/>. Enemies inside the
    /// overlap circle at the mid-point of the swing take <see cref="damage"/>.
    /// </summary>
    public class MeleeWeapon : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float damage = 25f;
        [SerializeField] private float range = 1.8f;
        [SerializeField] private float cooldown = 0.7f;

        [Header("Visual swing")]
        [SerializeField] private Transform swordPivot;
        [SerializeField] private float swingDuration = 0.22f;
        [SerializeField] private float swingStartAngle = 50f;
        [SerializeField] private float swingEndAngle = -80f;

        private AutoAttackWeapon _ranged;
        private float _timer;
        private bool _swinging;
        private float _swingTimer;
        private bool _damageApplied;
        private readonly Collider2D[] _buffer = new Collider2D[24];
        private readonly HashSet<Health> _hitThisSwing = new HashSet<Health>();

        private void Awake()
        {
            _ranged = GetComponent<AutoAttackWeapon>();
            if (_ranged != null) _ranged.enabled = false;

            _timer = cooldown; // ready immediately
        }

        private void Update()
        {
            if (GameManager.Instance != null && (GameManager.Instance.IsGameOver || GameManager.Instance.IsGameWon)) return;

            _timer += Time.deltaTime;

            if (!_swinging && _timer >= cooldown && HasNearbyEnemy())
            {
                _timer = 0f;
                BeginSwing();
            }

            if (_swinging)
                TickSwing();
        }

        // ── swing animation ───────────────────────────────────────────────

        private void BeginSwing()
        {
            _swinging = true;
            _swingTimer = 0f;
            _damageApplied = false;
            _hitThisSwing.Clear();
        }

        private void TickSwing()
        {
            _swingTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_swingTimer / swingDuration);

            if (swordPivot != null)
            {
                float angle = Mathf.LerpAngle(swingStartAngle, swingEndAngle, t);
                swordPivot.localEulerAngles = new Vector3(0f, 0f, angle);
            }

            // Deal damage at the mid-point of the swing arc
            if (!_damageApplied && t >= 0.5f)
            {
                ApplyDamage();
                _damageApplied = true;
            }

            if (t >= 1f)
                _swinging = false;
        }

        // ── damage ────────────────────────────────────────────────────────

        private void ApplyDamage()
        {
            int n = Physics2D.OverlapCircleNonAlloc(transform.position, range, _buffer);
            for (int i = 0; i < n; i++)
            {
                if (!_buffer[i].CompareTag("Enemy")) continue;
                var hp = _buffer[i].GetComponent<Health>();
                if (hp != null && !_hitThisSwing.Contains(hp))
                {
                    hp.TakeDamage(damage);
                    _hitThisSwing.Add(hp);
                }
            }
        }

        private bool HasNearbyEnemy()
        {
            int n = Physics2D.OverlapCircleNonAlloc(transform.position, range, _buffer);
            for (int i = 0; i < n; i++)
                if (_buffer[i].CompareTag("Enemy")) return true;
            return false;
        }

        // ── upgrade hooks ─────────────────────────────────────────────────

        public void UpgradeDamage(float mult) => damage *= mult;
        public void UpgradeRange(float mult) => range *= mult;
        public void UpgradeCooldown(float mult) => cooldown *= mult;
    }
}
