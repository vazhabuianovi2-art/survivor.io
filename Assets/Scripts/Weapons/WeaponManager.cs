using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Owns the player's set of acquirable auto-weapons (Survivor.io style).
    /// Holds a catalog of weapon types, equips them at runtime, and produces the
    /// weapon-related level-up choices (acquire new / level up existing).
    /// The starting sword (MeleeWeapon) lives outside this manager.
    /// </summary>
    public class WeaponManager : MonoBehaviour
    {
        public static WeaponManager Instance { get; private set; }

        [Tooltip("Max acquirable weapons (excluding the starting sword).")]
        [SerializeField] private int maxWeapons = 5;

        [Tooltip("Projectile prefab shared by ranged weapons (e.g. Magic Bolt).")]
        [SerializeField] private GameObject projectilePrefab;

        public GameObject ProjectilePrefab => projectilePrefab;

        private Transform _player;
        private readonly List<WeaponBase> _weapons = new List<WeaponBase>();

        /// <summary>Currently equipped weapons (for HUD display).</summary>
        public IReadOnlyList<WeaponBase> Weapons => _weapons;

        private struct WeaponDef
        {
            public string Name;
            public string AcquireDesc;
            public Type Type;
        }

        // Everything the player can pick up over a run.
        private readonly List<WeaponDef> _catalog = new List<WeaponDef>
        {
            new WeaponDef { Name = "Magic Bolt",   AcquireDesc = "Auto-fires bolts at the nearest enemy",   Type = typeof(MagicBoltWeapon) },
            new WeaponDef { Name = "Orbit Orbs",   AcquireDesc = "Orbs circle you, hurting what they touch", Type = typeof(OrbitOrbWeapon) },
            new WeaponDef { Name = "Kunai",        AcquireDesc = "Throws a wide fan of knives",              Type = typeof(KunaiWeapon) },
            new WeaponDef { Name = "Forcefield",   AcquireDesc = "A damaging aura surrounds you",            Type = typeof(ForcefieldWeapon) },
            new WeaponDef { Name = "Energy Laser", AcquireDesc = "Piercing beam through a line of enemies",  Type = typeof(EnergyLaserWeapon) },
            new WeaponDef { Name = "Drones",       AcquireDesc = "Companion drones that shoot for you",      Type = typeof(DroneWeapon) },
        };

        private struct EvolutionDef
        {
            public Type Base;
            public string Passive;
            public Type Evolved;
            public string Name;
            public string Desc;
        }

        // Maxed weapon + required passive → evolved form.
        private readonly List<EvolutionDef> _evolutions = new List<EvolutionDef>
        {
            new EvolutionDef { Base = typeof(MagicBoltWeapon),   Passive = "Power",    Evolved = typeof(ArcaneStormWeapon),  Name = "Arcane Storm",   Desc = "Magic Bolt → a barrage of bolts" },
            new EvolutionDef { Base = typeof(KunaiWeapon),       Passive = "Cooldown", Evolved = typeof(ShurikenStormWeapon),Name = "Shuriken Storm", Desc = "Kunai → a 360° storm of knives" },
            new EvolutionDef { Base = typeof(EnergyLaserWeapon), Passive = "Power",    Evolved = typeof(DeathRayWeapon),     Name = "Death Ray",      Desc = "Laser → a rapid piercing death ray" },
            new EvolutionDef { Base = typeof(OrbitOrbWeapon),    Passive = "Cooldown", Evolved = typeof(SaturnRingWeapon),   Name = "Saturn Ring",    Desc = "Orbs → a giant fast ring" },
            new EvolutionDef { Base = typeof(DroneWeapon),       Passive = "Power",    Evolved = typeof(DroneSwarmWeapon),   Name = "Drone Swarm",    Desc = "Drones → a fast-firing swarm" },
            new EvolutionDef { Base = typeof(ForcefieldWeapon),  Passive = "Vitality", Evolved = typeof(BlackHoleWeapon),    Name = "Black Hole",     Desc = "Forcefield → a pulling black hole" },
        };

        private PassiveManager _passives;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            _player = player != null ? player.transform : transform;
        }

        public bool HasFreeSlot => _weapons.Count < maxWeapons;

        private bool Owns(Type t)
        {
            foreach (var w in _weapons) if (w.GetType() == t) return true;
            return false;
        }

        public WeaponBase Acquire(Type type)
        {
            if (_player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                _player = p != null ? p.transform : transform;
            }

            var weapon = (WeaponBase)_player.gameObject.AddComponent(type);
            weapon.Initialize(_player);
            _weapons.Add(weapon);
            return weapon;
        }

        /// <summary>
        /// Build the weapon-related choices for a level-up: level up any owned,
        /// non-maxed weapon, plus acquire any catalog weapon if a slot is free.
        /// </summary>
        public List<Skill> BuildWeaponSkills()
        {
            var skills = new List<Skill>();

            foreach (var w in _weapons)
            {
                if (w.IsMaxed) continue;
                var weapon = w;
                skills.Add(new Skill(
                    $"{w.DisplayName} Lv{w.Level + 1}",
                    w.DescribeNext(),
                    () => weapon.LevelUp()));
            }

            if (HasFreeSlot)
            {
                foreach (var def in _catalog)
                {
                    if (Owns(def.Type)) continue;
                    var d = def;
                    skills.Add(new Skill(
                        $"New: {d.Name}",
                        d.AcquireDesc,
                        () => Acquire(d.Type)));
                }
            }

            // Evolution offers: a maxed base weapon + its required passive.
            if (_passives == null) _passives = FindFirstObjectByType<PassiveManager>();
            if (_passives != null)
            {
                foreach (var w in _weapons)
                {
                    if (w.IsEvolved || !w.IsMaxed) continue;
                    foreach (var ev in _evolutions)
                    {
                        if (ev.Base != w.GetType()) continue;
                        if (!_passives.HasPassive(ev.Passive)) continue;
                        if (Owns(ev.Evolved)) continue;

                        var baseW = w;
                        var def = ev;
                        skills.Add(new Skill(
                            $"★ EVOLVE: {def.Name}",
                            def.Desc,
                            () => Evolve(baseW, def.Evolved)));
                    }
                }
            }

            return skills;
        }

        private void Evolve(WeaponBase baseWeapon, Type evolvedType)
        {
            var owner = _player != null ? _player : baseWeapon.transform;
            _weapons.Remove(baseWeapon);
            Destroy(baseWeapon);

            var evolved = (WeaponBase)owner.gameObject.AddComponent(evolvedType);
            evolved.Initialize(owner);
            _weapons.Add(evolved);

            if (CameraFollow.Instance != null) CameraFollow.Instance.Shake(0.3f, 0.25f);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayLevelUp();
        }
    }
}
