using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Owns the player's passive items (up to 6) and produces their level-up
    /// choices, mirroring <see cref="WeaponManager"/> for weapons.
    /// </summary>
    public class PassiveManager : MonoBehaviour
    {
        public static PassiveManager Instance { get; private set; }

        [SerializeField] private int maxPassives = 6;

        private readonly List<PassiveItem> _owned = new List<PassiveItem>();

        /// <summary>Currently owned passives (for HUD display).</summary>
        public IReadOnlyList<PassiveItem> Owned => _owned;

        private struct PassiveDef
        {
            public string Name;
            public Func<PassiveItem> Create;
        }

        private readonly List<PassiveDef> _catalog = new List<PassiveDef>
        {
            new PassiveDef { Name = "Power",      Create = () => new PowerPassive() },
            new PassiveDef { Name = "Cooldown",   Create = () => new CooldownPassive() },
            new PassiveDef { Name = "Swift Boots",Create = () => new BootsPassive() },
            new PassiveDef { Name = "Magnet",     Create = () => new MagnetPassive() },
            new PassiveDef { Name = "Wisdom",     Create = () => new WisdomPassive() },
            new PassiveDef { Name = "Vitality",   Create = () => new VitalityPassive() },
        };

        private void Awake() => Instance = this;

        public bool HasFreeSlot => _owned.Count < maxPassives;

        private bool Owns(string name)
        {
            foreach (var p in _owned) if (p.DisplayName == name) return true;
            return false;
        }

        /// <summary>Public check used by weapon-evolution requirements.</summary>
        public bool HasPassive(string name) => Owns(name);

        public List<Skill> BuildPassiveSkills()
        {
            var skills = new List<Skill>();

            foreach (var p in _owned)
            {
                if (p.IsMaxed) continue;
                var passive = p;
                skills.Add(new Skill(
                    $"{p.DisplayName} Lv{p.Level + 1}",
                    p.DescribeNext(),
                    () => passive.LevelUp()));
            }

            if (HasFreeSlot)
            {
                foreach (var def in _catalog)
                {
                    // Match by the display name produced by a throwaway instance.
                    var sample = def.Create();
                    if (Owns(sample.DisplayName)) continue;
                    var d = def;
                    skills.Add(new Skill(
                        $"New: {sample.DisplayName}",
                        sample.DescribeNext(),
                        () => Acquire(d.Create())));
                }
            }

            return skills;
        }

        private void Acquire(PassiveItem item)
        {
            item.Acquire();
            _owned.Add(item);
        }
    }
}
