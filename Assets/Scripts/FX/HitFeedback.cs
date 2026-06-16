using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Adds "juice" when this object's <see cref="Health"/> takes damage:
    /// a quick scale-punch, a floating damage number, and (optionally) a
    /// camera shake. Attach to any object that has a Health component.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class HitFeedback : MonoBehaviour
    {
        [Header("Scale punch")]
        [SerializeField] private Transform punchTarget;     // defaults to this transform
        [SerializeField] private float punchAmount = 0.18f;
        [SerializeField] private float punchDuration = 0.12f;

        [Header("Damage number")]
        [SerializeField] private bool showPopup = true;
        [SerializeField] private Color popupColor = new Color(1f, 0.95f, 0.4f);

        [Header("Camera shake (use on the player)")]
        [SerializeField] private bool shakeOnHit = false;
        [SerializeField] private float shakeMagnitude = 0.18f;

        private Health _health;
        private Vector3 _baseScale;
        private float _punchTime;

        private void Awake()
        {
            _health = GetComponent<Health>();
            if (punchTarget == null) punchTarget = transform;
            _baseScale = punchTarget.localScale;
        }

        private void OnEnable()  => _health.Damaged += OnDamaged;
        private void OnDisable() => _health.Damaged -= OnDamaged;

        private void OnDamaged(Health h, float amount)
        {
            if (showPopup) DamagePopup.Spawn(transform.position, amount, popupColor);
            _punchTime = punchDuration;
            if (shakeOnHit && CameraFollow.Instance != null)
                CameraFollow.Instance.Shake(shakeMagnitude, 0.15f);

            if (AudioManager.Instance != null)
            {
                if (shakeOnHit) AudioManager.Instance.PlayHurt();   // player took damage
                else            AudioManager.Instance.PlayHit();    // enemy was hit
            }
        }

        private void Update()
        {
            if (_punchTime > 0f)
            {
                _punchTime -= Time.deltaTime;
                float t = Mathf.Clamp01(_punchTime / punchDuration);   // 1 → 0
                punchTarget.localScale = _baseScale * (1f + punchAmount * t);
                if (_punchTime <= 0f) punchTarget.localScale = _baseScale;
            }
        }
    }
}
