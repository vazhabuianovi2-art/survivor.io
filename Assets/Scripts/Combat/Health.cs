using System;
using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Generic health pool used by enemies (and later the player).
    /// Other systems deal damage via <see cref="TakeDamage"/> and react to the
    /// <see cref="Died"/> / <see cref="Damaged"/> events.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 3f;

        public float Max => maxHealth;
        public float Current { get; private set; }
        public bool IsDead { get; private set; }

        /// <summary>Raised once when health reaches zero.</summary>
        public event Action<Health> Died;
        /// <summary>Raised whenever damage is applied (passes the amount).</summary>
        public event Action<Health, float> Damaged;

        private void Awake()
        {
            Current = maxHealth;
        }

        public void SetMaxHealth(float value, bool refill = true)
        {
            maxHealth = Mathf.Max(1f, value);
            if (refill) Current = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f) return;

            Current -= amount;
            Damaged?.Invoke(this, amount);

            if (Current <= 0f)
            {
                Current = 0f;
                IsDead = true;
                Died?.Invoke(this);
            }
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;
            Current = Mathf.Min(maxHealth, Current + amount);
        }

        /// <summary>Bring a dead entity back to life with the given health (used by Revive).</summary>
        public void Revive(float toHealth)
        {
            IsDead = false;
            Current = Mathf.Clamp(toHealth, 1f, maxHealth);
        }
    }
}
