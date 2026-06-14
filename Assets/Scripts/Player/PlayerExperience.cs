using System;
using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Tracks the player's experience and level. XP is added by collecting gems;
    /// when the bar fills the player levels up (possibly several times for a big
    /// pickup). Other systems listen to <see cref="LeveledUp"/> / <see cref="Changed"/>.
    /// </summary>
    public class PlayerExperience : MonoBehaviour
    {
        [SerializeField] private float baseXpToNext = 5f;
        [Tooltip("Multiplier applied to the requirement each level.")]
        [SerializeField] private float growth = 1.3f;

        public int Level { get; private set; } = 1;
        public float Xp { get; private set; }
        public float XpToNext { get; private set; }

        /// <summary>Raised once per level gained, passing the new level.</summary>
        public event Action<int> LeveledUp;
        /// <summary>Raised whenever XP or level changes (for the XP bar UI).</summary>
        public event Action Changed;

        private void Awake()
        {
            XpToNext = baseXpToNext;
        }

        public void AddXp(float amount)
        {
            if (amount <= 0f) return;
            Xp += amount;

            while (Xp >= XpToNext)
            {
                Xp -= XpToNext;
                Level++;
                XpToNext = Mathf.Ceil(baseXpToNext * Mathf.Pow(growth, Level - 1));
                LeveledUp?.Invoke(Level);
            }
            Changed?.Invoke();
        }
    }
}
