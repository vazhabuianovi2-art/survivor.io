using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Smoothly follows a target (the player) keeping a fixed camera depth.
    /// Designed for a top-down 2D orthographic camera.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [Tooltip("Higher = snappier follow. Lower = smoother lag.")]
        [SerializeField] private float smoothSpeed = 8f;
        [SerializeField] private Vector2 offset = Vector2.zero;

        private float _depth;

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

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = new Vector3(
                target.position.x + offset.x,
                target.position.y + offset.y,
                _depth);

            transform.position = Vector3.Lerp(
                transform.position, desired, smoothSpeed * Time.deltaTime);
        }
    }
}
