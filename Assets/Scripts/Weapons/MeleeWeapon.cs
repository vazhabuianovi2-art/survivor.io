using UnityEngine;

namespace SurvivorIO
{
    public class MeleeWeapon : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float damage = 25f;
        [SerializeField] private float range = 5f;
        [SerializeField] private float cooldown = 0.7f;

        [Header("Sword hold")]
        [SerializeField] private Transform swordPivot;
        [SerializeField] private float restAngle = -30f;

        [Header("Slash projectile")]
        [SerializeField] private GameObject slashPrefab;
        [SerializeField] private float slashDuration = 0.18f;

        private float _timer;
        private bool _attacking;
        private float _attackTimer;
        private bool _slashFired;
        private Vector2 _attackDir;

        private readonly Collider2D[] _buffer = new Collider2D[24];

        private void Awake()
        {
            var ranged = GetComponent<AutoAttackWeapon>();
            if (ranged != null) ranged.enabled = false;

            _timer = cooldown;
            if (swordPivot != null)
                swordPivot.localEulerAngles = new Vector3(0f, 0f, restAngle);
        }

        private void Update()
        {
            if (GameManager.Instance != null && (GameManager.Instance.IsGameOver || GameManager.Instance.IsGameWon))
                return;

            if (_attacking)
            {
                _attackTimer += Time.deltaTime;
                float t = Mathf.Clamp01(_attackTimer / slashDuration);

                if (swordPivot != null)
                {
                    float angle = t < 0.5f
                        ? Mathf.Lerp(restAngle, restAngle + 60f, t * 2f)
                        : Mathf.Lerp(restAngle + 60f, restAngle, (t - 0.5f) * 2f);
                    swordPivot.localEulerAngles = new Vector3(0f, 0f, angle);
                }

                if (!_slashFired && t >= 0.5f)
                {
                    _slashFired = true;
                    SpawnSlash();
                }

                if (t >= 1f)
                {
                    _attacking = false;
                    if (swordPivot != null)
                        swordPivot.localEulerAngles = new Vector3(0f, 0f, restAngle);
                }
                return;
            }

            _timer += Time.deltaTime;
            if (_timer >= cooldown)
            {
                var nearest = FindNearest();
                if (nearest != null)
                {
                    _timer = 0f;
                    _attackDir = (nearest.position - transform.position).normalized;
                    _attacking = true;
                    _attackTimer = 0f;
                    _slashFired = false;
                }
            }
        }

        private void SpawnSlash()
        {
            if (slashPrefab == null) return;
            var go = Instantiate(slashPrefab, transform.position, Quaternion.identity);
            var sp = go.GetComponent<SlashProjectile>();
            if (sp != null) sp.Init(damage, _attackDir);
        }

        private Transform FindNearest()
        {
            int n = Physics2D.OverlapCircleNonAlloc(transform.position, range, _buffer);
            Transform nearest = null;
            float best = float.MaxValue;
            for (int i = 0; i < n; i++)
            {
                if (!_buffer[i].CompareTag("Enemy")) continue;
                float d = ((Vector2)(_buffer[i].transform.position - transform.position)).sqrMagnitude;
                if (d < best) { best = d; nearest = _buffer[i].transform; }
            }
            return nearest;
        }

        public float CooldownProgress => _attacking ? 1f : Mathf.Clamp01(_timer / cooldown);

        public void UpgradeDamage(float mult) => damage *= mult;
        public void UpgradeRange(float mult) => range *= mult;
        public void UpgradeCooldown(float mult) => cooldown *= mult;
    }
}
