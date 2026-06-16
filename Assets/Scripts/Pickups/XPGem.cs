using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Experience pickup dropped by enemies. Once the player comes within the
    /// pickup radius the gem flies toward them (magnet) and grants XP on contact.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class XPGem : MonoBehaviour
    {
        [SerializeField] private float value = 1f;
        [SerializeField] private float pickupRadius = 2.5f;
        [SerializeField] private float magnetSpeed = 14f;
        [SerializeField] private float collectDistance = 0.35f;

        private Transform _player;
        private PlayerExperience _xp;
        private bool _magnetized;

        public void SetValue(float v) => value = v;

        /// <summary>Force this gem to fly to the player (used by the Magnet pickup).</summary>
        public void PullNow() => _magnetized = true;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _player = player.transform;
                _xp = player.GetComponent<PlayerExperience>();
            }
        }

        private void Update()
        {
            if (_player == null) return;

            float dist = Vector2.Distance(transform.position, _player.position);
            if (!_magnetized && dist <= pickupRadius * PlayerStats.Pickup)
                _magnetized = true;

            if (_magnetized)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position, _player.position, magnetSpeed * Time.deltaTime);

                if (dist <= collectDistance)
                    Collect();
            }
        }

        private void Collect()
        {
            if (_xp != null) _xp.AddXp(value);
            Destroy(gameObject);
        }
    }
}
