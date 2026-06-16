namespace SurvivorIO
{
    /// <summary>
    /// A passive upgrade that modifies global <see cref="PlayerStats"/> rather than
    /// firing. Levels 1-5; each level applies an incremental stat delta.
    /// </summary>
    public abstract class PassiveItem
    {
        public const int MaxLevel = 5;
        public int Level { get; private set; }
        public bool IsMaxed => Level >= MaxLevel;

        public abstract string DisplayName { get; }
        public abstract string DescribeNext();

        public void Acquire()
        {
            Level = 1;
            Apply(1);
        }

        public void LevelUp()
        {
            if (IsMaxed) return;
            Level++;
            Apply(Level);
        }

        /// <summary>Apply the incremental effect for reaching <paramref name="level"/>.</summary>
        protected abstract void Apply(int level);
    }

    public class PowerPassive : PassiveItem
    {
        public override string DisplayName => "Power";
        public override string DescribeNext() => "+15% all damage";
        protected override void Apply(int level)
        {
            if (PlayerStats.Instance != null) PlayerStats.Instance.DamageMult += 0.15f;
        }
    }

    public class CooldownPassive : PassiveItem
    {
        public override string DisplayName => "Cooldown";
        public override string DescribeNext() => "-8% all cooldowns";
        protected override void Apply(int level)
        {
            if (PlayerStats.Instance != null) PlayerStats.Instance.CooldownMult *= 0.92f;
        }
    }

    public class BootsPassive : PassiveItem
    {
        public override string DisplayName => "Swift Boots";
        public override string DescribeNext() => "+12% move speed";
        protected override void Apply(int level)
        {
            if (PlayerStats.Instance != null) PlayerStats.Instance.MoveSpeedMult += 0.12f;
        }
    }

    public class MagnetPassive : PassiveItem
    {
        public override string DisplayName => "Magnet";
        public override string DescribeNext() => "+30% pickup range";
        protected override void Apply(int level)
        {
            if (PlayerStats.Instance != null) PlayerStats.Instance.PickupRangeMult += 0.30f;
        }
    }

    public class WisdomPassive : PassiveItem
    {
        public override string DisplayName => "Wisdom";
        public override string DescribeNext() => "+20% XP gain";
        protected override void Apply(int level)
        {
            if (PlayerStats.Instance != null) PlayerStats.Instance.XpMult += 0.20f;
        }
    }

    public class VitalityPassive : PassiveItem
    {
        public override string DisplayName => "Vitality";
        public override string DescribeNext() => "+25 max HP";
        protected override void Apply(int level)
        {
            if (PlayerStats.Instance != null) PlayerStats.Instance.AddMaxHp(25f);
        }
    }
}
