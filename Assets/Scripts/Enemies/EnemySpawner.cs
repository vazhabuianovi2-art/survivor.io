using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Spawns enemies on a ring just outside the camera view around the player.
    /// The spawn interval ramps from <see cref="startInterval"/> down to
    /// <see cref="minInterval"/> over <see cref="rampDuration"/> seconds, so the
    /// pressure increases the longer the run lasts.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Transform target;

        [Header("Spawn placement")]
        [Tooltip("Fallback spawn distance if no camera is found.")]
        [SerializeField] private float spawnRadius = 14f;
        [Tooltip("Extra distance beyond the screen corner so enemies appear just off-screen.")]
        [SerializeField] private float spawnMargin = 2f;

        [Header("Difficulty ramp")]
        [SerializeField] private float startInterval = 1.5f;
        [SerializeField] private float minInterval = 0.25f;
        [SerializeField] private float rampDuration = 120f;
        [Tooltip("How many enemies can spawn per tick once warmed up.")]
        [SerializeField] private int spawnBatch = 1;
        [SerializeField] private int maxAlive = 250;

        private float _timer;
        private float _elapsed;
        private Camera _cam;

        private void Start()
        {
            if (target == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) target = player.transform;
            }
            _cam = Camera.main;
            _timer = startInterval;
        }

        /// <summary>
        /// Distance at which to spawn so enemies always appear just outside the
        /// visible area, regardless of zoom or aspect ratio (the screen corner).
        /// </summary>
        private float SpawnDistance()
        {
            if (_cam != null && _cam.orthographic)
            {
                float halfH = _cam.orthographicSize;
                float halfW = halfH * _cam.aspect;
                return Mathf.Sqrt(halfH * halfH + halfW * halfW) + spawnMargin;
            }
            return spawnRadius;
        }

        private void Update()
        {
            if (enemyPrefab == null || target == null) return;

            _elapsed += Time.deltaTime;
            _timer -= Time.deltaTime;

            if (_timer <= 0f)
            {
                _timer = CurrentInterval();
                for (int i = 0; i < spawnBatch && Enemy.AliveCount < maxAlive; i++)
                    SpawnOne();
            }
        }

        private float CurrentInterval()
        {
            float t = Mathf.Clamp01(_elapsed / rampDuration);
            return Mathf.Lerp(startInterval, minInterval, t);
        }

        private void SpawnOne()
        {
            float angle = Random.value * Mathf.PI * 2f;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * SpawnDistance();
            Vector2 pos = (Vector2)target.position + offset;
            Instantiate(enemyPrefab, pos, Quaternion.identity);
        }
    }
}
