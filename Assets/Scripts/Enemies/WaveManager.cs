using UnityEngine;

namespace SurvivorIO
{
    [System.Serializable]
    public class EnemyWaveConfig
    {
        public string label;
        public GameObject prefab;
        [Tooltip("Seconds from run start when this enemy type begins spawning.")]
        public float unlockTime = 0f;
        [Tooltip("Starting spawn interval (seconds).")]
        public float startInterval = 2f;
        [Tooltip("Minimum interval after ramp.")]
        public float minInterval = 0.4f;
        [Tooltip("Time (seconds) to reach minInterval from startInterval.")]
        public float rampDuration = 120f;
        [Tooltip("Max of this type alive simultaneously.")]
        public int maxAlive = 80;
    }

    /// <summary>
    /// Manages all enemy waves. Replaces EnemySpawner for multi-type support.
    /// Timeline:
    ///   0s   → Goblins
    ///   180s → Skeletons join
    ///   300s → Orc/Bat join + MiniBoss single spawn
    ///   540s → Stronger everything
    ///   900s → Boss spawns once; on death → WIN
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [Header("Enemy Wave Configs")]
        [SerializeField] private EnemyWaveConfig[] waves;

        [Header("Special spawns")]
        [SerializeField] private GameObject miniBossPrefab;
        [SerializeField] private float miniBossTime = 300f;
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private float bossTime = 900f;

        [Header("Spawn placement")]
        [SerializeField] private float spawnMargin = 2f;

        [Header("Elites")]
        [Tooltip("Seconds between elite spawns (tough, gold-tinted, drops a chest).")]
        [SerializeField] private float eliteInterval = 40f;
        [Tooltip("Elites start appearing after this time.")]
        [SerializeField] private float eliteStartTime = 30f;

        private Transform _target;
        private Camera _cam;
        private float[] _timers;
        private bool _miniBossSpawned;
        private bool _bossSpawned;
        private float _elapsed;
        private float _eliteTimer;

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) _target = player.transform;
            _cam = Camera.main;
            _timers = new float[waves.Length];
            for (int i = 0; i < waves.Length; i++)
                _timers[i] = waves[i].startInterval;
        }

        private void Update()
        {
            if (GameManager.Instance != null && (GameManager.Instance.IsGameOver || GameManager.Instance.IsGameWon)) return;
            if (_target == null) return;

            _elapsed = GameManager.Instance != null ? GameManager.Instance.ElapsedTime : Time.time;

            for (int i = 0; i < waves.Length; i++)
            {
                var w = waves[i];
                if (w.prefab == null || _elapsed < w.unlockTime) continue;

                _timers[i] -= Time.deltaTime;
                if (_timers[i] <= 0f)
                {
                    _timers[i] = CurrentInterval(w, _elapsed - w.unlockTime);
                    if (Enemy.AliveCount < w.maxAlive)
                        SpawnAt(w.prefab, RandomSpawnPos());
                }
            }

            // Elites (periodic)
            if (_elapsed >= eliteStartTime)
            {
                _eliteTimer += Time.deltaTime;
                if (_eliteTimer >= eliteInterval)
                {
                    _eliteTimer = 0f;
                    SpawnElite();
                }
            }

            // Mini-boss (once)
            if (!_miniBossSpawned && miniBossPrefab != null && _elapsed >= miniBossTime)
            {
                _miniBossSpawned = true;
                SpawnAt(miniBossPrefab, RandomSpawnPos());
            }

            // Boss (once)
            if (!_bossSpawned && bossPrefab != null && _elapsed >= bossTime)
            {
                _bossSpawned = true;
                var bossGo = SpawnAt(bossPrefab, RandomSpawnPos());
                var bossHp = bossGo.GetComponent<Health>();
                if (bossHp != null)
                {
                    bossHp.Died += _ => GameManager.Instance?.TriggerWin();
                    BossHealthBar.Instance?.SetBoss(bossHp, "DARK KNIGHT");
                }
            }
        }

        private void SpawnElite()
        {
            GameObject prefab = null;
            for (int tries = 0; tries < 8 && prefab == null; tries++)
            {
                var w = waves[Random.Range(0, waves.Length)];
                if (w.prefab != null && _elapsed >= w.unlockTime) prefab = w.prefab;
            }
            if (prefab == null && waves.Length > 0) prefab = waves[0].prefab;
            if (prefab == null) return;

            var go = SpawnAt(prefab, RandomSpawnPos());
            var enemy = go.GetComponent<Enemy>();
            if (enemy != null) enemy.MakeElite();
        }

        private static float CurrentInterval(EnemyWaveConfig w, float timeInWave)
        {
            float t = Mathf.Clamp01(timeInWave / w.rampDuration);
            return Mathf.Lerp(w.startInterval, w.minInterval, t);
        }

        private GameObject SpawnAt(GameObject prefab, Vector2 pos)
        {
            return Instantiate(prefab, pos, Quaternion.identity);
        }

        private Vector2 RandomSpawnPos()
        {
            float dist = SpawnDistance();
            float angle = Random.value * Mathf.PI * 2f;
            return (Vector2)_target.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
        }

        private float SpawnDistance()
        {
            if (_cam != null && _cam.orthographic)
            {
                float h = _cam.orthographicSize;
                float w = h * _cam.aspect;
                return Mathf.Sqrt(h * h + w * w) + spawnMargin;
            }
            return 14f;
        }
    }
}
