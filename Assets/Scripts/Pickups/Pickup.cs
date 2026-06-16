using UnityEngine;

namespace SurvivorIO
{
    public enum PickupType { Gold, Magnet, Bomb, Heal, Chest }

    /// <summary>
    /// A collectible dropped by enemies. Magnetises to the player when close and
    /// applies a type-specific effect on pickup. Built entirely in code via
    /// <see cref="Spawn"/> — no prefab needed.
    /// </summary>
    public class Pickup : MonoBehaviour
    {
        private const float BombDamage = 500f;

        private PickupType _type;
        private float _value;
        private float _pickupRadius = 1.6f;
        private float _magnetSpeed = 12f;
        private float _collectDistance = 0.4f;

        private Transform _player;
        private bool _magnetized;

        public static Pickup Spawn(PickupType type, Vector3 pos, float value = 0f)
        {
            var go = new GameObject($"Pickup_{type}");
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = FxSprites.Tinted(ColorFor(type));
            sr.sortingOrder = 8;
            go.transform.localScale = Vector3.one * (type == PickupType.Chest ? 0.9f : 0.5f);

            var p = go.AddComponent<Pickup>();
            p._type = type;
            p._value = value;
            if (type == PickupType.Chest) p._pickupRadius = 1.2f;
            return p;
        }

        private static Color ColorFor(PickupType t)
        {
            switch (t)
            {
                case PickupType.Magnet: return new Color(0.3f, 0.7f, 1f);
                case PickupType.Bomb:   return new Color(1f, 0.4f, 0.1f);
                case PickupType.Heal:   return new Color(0.3f, 1f, 0.4f);
                case PickupType.Chest:  return new Color(1f, 0.8f, 0.25f);
                default:                return new Color(1f, 0.85f, 0.2f);   // Gold
            }
        }

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) _player = player.transform;
        }

        private void Update()
        {
            if (_player == null) return;

            float dist = Vector2.Distance(transform.position, _player.position);
            if (!_magnetized && dist <= _pickupRadius * PlayerStats.Pickup)
                _magnetized = true;

            if (_magnetized)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position, _player.position, _magnetSpeed * Time.deltaTime);
                if (dist <= _collectDistance)
                    Collect();
            }
        }

        private void Collect()
        {
            switch (_type)
            {
                case PickupType.Gold:
                    GameManager.Instance?.AddGold(Mathf.Max(1, Mathf.RoundToInt(_value)));
                    break;

                case PickupType.Magnet:
                    foreach (var gem in FindObjectsByType<XPGem>(FindObjectsSortMode.None))
                        gem.PullNow();
                    break;

                case PickupType.Bomb:
                    ScreenClear();
                    if (CameraFollow.Instance != null) CameraFollow.Instance.Shake(0.35f, 0.25f);
                    break;

                case PickupType.Heal:
                    HealPlayer(_value > 0f ? _value : 40f);
                    break;

                case PickupType.Chest:
                    GameManager.Instance?.AddGold(50);
                    HealPlayer(30f);
                    _player.GetComponent<PlayerExperience>()?.GrantBonusLevel();
                    break;
            }

            if (AudioManager.Instance != null) AudioManager.Instance.PlayLevelUp();
            Destroy(gameObject);
        }

        private void HealPlayer(float amount)
        {
            var hp = _player.GetComponent<Health>();
            if (hp != null) hp.Heal(amount);
        }

        private void ScreenClear()
        {
            var hits = Physics2D.OverlapCircleAll(_player.position, 30f);
            foreach (var h in hits)
            {
                if (!h.CompareTag("Enemy")) continue;
                h.GetComponent<Health>()?.TakeDamage(BombDamage);
            }
        }
    }
}
