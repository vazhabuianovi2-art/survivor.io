using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// On death, spawns several smaller enemies. The spawn prefab should NOT have
    /// a SplitterBehavior itself (to avoid infinite splitting).
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    [RequireComponent(typeof(Health))]
    public class SplitterBehavior : MonoBehaviour
    {
        [SerializeField] private GameObject spawnPrefab;
        [SerializeField] private int count = 3;
        [SerializeField] private float scale = 0.6f;

        private Health _health;
        private bool _spawned;

        private void Awake() => _health = GetComponent<Health>();
        private void OnEnable()  => _health.Died += OnDied;
        private void OnDisable() => _health.Died -= OnDied;

        private void OnDied(Health h)
        {
            if (_spawned || spawnPrefab == null) return;
            _spawned = true;

            for (int i = 0; i < count; i++)
            {
                Vector3 off = Random.insideUnitCircle * 0.6f;
                var go = Instantiate(spawnPrefab, transform.position + off, Quaternion.identity);
                go.transform.localScale *= scale;
            }
        }
    }
}
