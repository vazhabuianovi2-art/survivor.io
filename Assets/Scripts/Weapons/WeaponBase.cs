using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Base for all Survivor-style auto weapons. A weapon is a component added to
    /// the player at runtime by <see cref="WeaponManager"/>. It fires on its own
    /// (each subclass implements its own <c>Update</c>), levels from 1 to
    /// <see cref="MaxLevel"/>, and exposes a short description of what the next
    /// level grants for the level-up UI.
    /// </summary>
    public abstract class WeaponBase : MonoBehaviour
    {
        public virtual int MaxLevel => 5;

        public int Level { get; private set; } = 1;
        public bool IsMaxed => Level >= MaxLevel;

        /// <summary>True for evolved weapons — they never appear as level-up offers.</summary>
        public virtual bool IsEvolved => false;

        /// <summary>Name shown on level-up cards and HUD.</summary>
        public abstract string DisplayName { get; }

        /// <summary>One-line description of what acquiring / the next level grants.</summary>
        public abstract string DescribeNext();

        protected Transform Owner;
        private static readonly Collider2D[] _buf = new Collider2D[128];

        /// <summary>Called once when the weapon is first equipped.</summary>
        public void Initialize(Transform owner)
        {
            Owner = owner;
            OnEquip();
        }

        public void LevelUp()
        {
            if (IsMaxed) return;
            Level++;
            OnLevelUp();
        }

        protected virtual void OnEquip() { }
        protected virtual void OnLevelUp() { }

        protected bool GameActive =>
            GameManager.Instance == null ||
            (!GameManager.Instance.IsGameOver && !GameManager.Instance.IsGameWon);

        // ---- shared targeting helpers ----

        protected Transform FindNearestEnemy(float range)
        {
            if (Owner == null) return null;
            Vector2 origin = Owner.position;
            int n = Physics2D.OverlapCircleNonAlloc(origin, range, _buf);
            Transform best = null;
            float bestSqr = float.MaxValue;
            for (int i = 0; i < n; i++)
            {
                var c = _buf[i];
                if (c == null || !c.CompareTag("Enemy")) continue;
                float d = ((Vector2)c.transform.position - origin).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = c.transform; }
            }
            return best;
        }
    }
}
