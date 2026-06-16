using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Central hub of global player multipliers that passive items modify and
    /// weapons / movement / pickups read. One per player.
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats Instance { get; private set; }

        public float DamageMult     = 1f;   // scales all weapon damage
        public float CooldownMult   = 1f;   // scales all fire intervals (lower = faster)
        public float MoveSpeedMult  = 1f;
        public float PickupRangeMult = 1f;  // XP gem magnet range
        public float XpMult         = 1f;

        private Health _health;

        private void Awake()
        {
            Instance = this;
            _health = GetComponent<Health>();
        }

        /// <summary>Raise max HP and heal by the same amount (used by the Vitality passive).</summary>
        public void AddMaxHp(float amount)
        {
            if (_health == null) _health = GetComponent<Health>();
            if (_health == null) return;
            _health.SetMaxHealth(_health.Max + amount, refill: false);
            _health.Heal(amount);
        }

        // Convenience accessors with null-safety for callers.
        public static float Damage    => Instance != null ? Instance.DamageMult : 1f;
        public static float Cooldown  => Instance != null ? Instance.CooldownMult : 1f;
        public static float MoveSpeed => Instance != null ? Instance.MoveSpeedMult : 1f;
        public static float Pickup    => Instance != null ? Instance.PickupRangeMult : 1f;
        public static float Xp        => Instance != null ? Instance.XpMult : 1f;
    }
}
