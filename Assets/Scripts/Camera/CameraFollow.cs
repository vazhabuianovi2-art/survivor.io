using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Smoothly follows a target (the player) keeping a fixed camera depth.
    /// Designed for a top-down 2D orthographic camera.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        public static CameraFollow Instance { get; private set; }

        [SerializeField] private Transform target;
        [Tooltip("Higher = snappier follow. Lower = smoother lag.")]
        [SerializeField] private float smoothSpeed = 8f;
        [SerializeField] private Vector2 offset = Vector2.zero;

        private float _depth;

        // --- screen shake state ---
        private float _shakeTime;
        private float _shakeDuration;
        private float _shakeMagnitude;

        private void Awake() => Instance = this;

        private void Start()
        {
            _depth = transform.position.z;
            if (target == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) target = player.transform;
            }
        }

        public void SetTarget(Transform t) => target = t;

        /// <summary>Kick off a brief positional shake (e.g. on the player taking a hit).</summary>
        public void Shake(float magnitude = 0.2f, float duration = 0.15f)
        {
            // Don't let a weaker shake cut off a stronger one already playing.
            if (_shakeTime > 0f && magnitude < _shakeMagnitude) return;
            _shakeMagnitude = magnitude;
            _shakeDuration  = duration;
            _shakeTime      = duration;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = new Vector3(
                target.position.x + offset.x,
                target.position.y + offset.y,
                _depth);

            Vector3 pos = Vector3.Lerp(
                transform.position, desired, smoothSpeed * Time.deltaTime);

            if (_shakeTime > 0f)
            {
                _shakeTime -= Time.deltaTime;
                float falloff = _shakeDuration > 0f ? _shakeTime / _shakeDuration : 0f;
                Vector2 jitter = Random.insideUnitCircle * (_shakeMagnitude * Mathf.Clamp01(falloff));
                pos.x += jitter.x;
                pos.y += jitter.y;
            }

            transform.position = pos;
        }
    }
}
