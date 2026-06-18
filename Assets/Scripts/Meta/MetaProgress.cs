using System;
using System.IO;
using UnityEngine;

namespace SurvivorIO
{
    /// <summary>Serialized save payload (JSON on disk).</summary>
    [Serializable]
    public class SaveData
    {
        public int gold;
        public int[] upgradeLevels;
        public int selectedCharacter;
        public int charUnlockMask = 1;    // bit 0 = first character unlocked by default
        public int selectedStage;
        public int stageUnlockMask = 1;   // bit 0 = first stage unlocked by default
    }

    /// <summary>
    /// Persistent meta-progression: banked gold, permanent upgrade levels and
    /// character selection. Saved to a JSON file and applied to the player at the
    /// start of each run. Static service — lazy-loads on first access.
    /// </summary>
    public static class MetaProgress
    {
        public struct UpgradeDef
        {
            public string Name, Desc;
            public int MaxLevel, BaseCost;
        }

        public struct CharacterDef
        {
            public string Name, Desc;
            public float DmgBonus, HpBonus, SpeedBonus;
            public int UnlockCost;
        }

        public struct StageDef
        {
            public string Name, Desc;
            public float Difficulty;   // enemy HP/damage multiplier
            public Color Tint;         // background tint
            public int UnlockCost;
        }

        public static readonly UpgradeDef[] Upgrades =
        {
            new UpgradeDef { Name = "Might",  Desc = "+5% damage",        MaxLevel = 10, BaseCost = 50 },
            new UpgradeDef { Name = "Vigor",  Desc = "+15 max HP",        MaxLevel = 10, BaseCost = 50 },
            new UpgradeDef { Name = "Greed",  Desc = "+10% gold gain",    MaxLevel = 5,  BaseCost = 40 },
            new UpgradeDef { Name = "Haste",  Desc = "+3% move speed",    MaxLevel = 5,  BaseCost = 60 },
            new UpgradeDef { Name = "Growth", Desc = "+5% XP gain",       MaxLevel = 5,  BaseCost = 60 },
            new UpgradeDef { Name = "Armor",  Desc = "-3% damage taken",  MaxLevel = 5,  BaseCost = 80 },
        };

        public static readonly CharacterDef[] Characters =
        {
            new CharacterDef { Name = "Knight", Desc = "Sturdy all-rounder (+20 HP)",   DmgBonus = 0f,    HpBonus = 20f,  SpeedBonus = 0f,    UnlockCost = 0 },
            new CharacterDef { Name = "Rogue",  Desc = "Fast striker (+10% dmg/speed)", DmgBonus = 0.10f, HpBonus = 0f,   SpeedBonus = 0.10f, UnlockCost = 300 },
            new CharacterDef { Name = "Mage",   Desc = "Glass cannon (+20% dmg, -HP)",  DmgBonus = 0.20f, HpBonus = -10f, SpeedBonus = 0f,    UnlockCost = 500 },
        };

        public static readonly StageDef[] Stages =
        {
            new StageDef { Name = "Lava Fields", Desc = "The starting battleground",   Difficulty = 1f,   Tint = Color.white,                     UnlockCost = 0 },
            new StageDef { Name = "Frostlands",  Desc = "Tougher foes (+50%)",          Difficulty = 1.5f, Tint = new Color(0.6f, 0.8f, 1f),       UnlockCost = 400 },
            new StageDef { Name = "Inferno",     Desc = "Brutal (+120%)",               Difficulty = 2.2f, Tint = new Color(1f, 0.55f, 0.45f),     UnlockCost = 800 },
        };

        public static int UpgradeCount => Upgrades.Length;

        private static SaveData _data;
        private static string Path => System.IO.Path.Combine(Application.persistentDataPath, "survivorio_save.json");

        public static SaveData Data
        {
            get { if (_data == null) Load(); return _data; }
        }

        private static void Load()
        {
            try
            {
                if (File.Exists(Path))
                    _data = JsonUtility.FromJson<SaveData>(File.ReadAllText(Path));
            }
            catch (Exception e) { Debug.LogWarning("Save load failed: " + e.Message); }

            if (_data == null) _data = new SaveData();
            if (_data.upgradeLevels == null || _data.upgradeLevels.Length < UpgradeCount)
            {
                var arr = new int[UpgradeCount];
                if (_data.upgradeLevels != null)
                    Array.Copy(_data.upgradeLevels, arr, Mathf.Min(_data.upgradeLevels.Length, UpgradeCount));
                _data.upgradeLevels = arr;
            }
        }

        public static void Save()
        {
            try { File.WriteAllText(Path, JsonUtility.ToJson(Data, true)); }
            catch (Exception e) { Debug.LogWarning("Save write failed: " + e.Message); }
        }

        // ---- gold ----
        public static int Gold => Data.gold;

        /// <summary>Add this run's collected gold to the bank. The Greed multiplier
        /// is applied at collection time, so run gold is banked as-is here.</summary>
        public static void BankRunGold(int amount)
        {
            Data.gold += Mathf.Max(0, amount);
            Save();
        }

        // ---- upgrades ----
        public static int LevelOf(int id) => Data.upgradeLevels[id];
        public static int CostOf(int id) => Upgrades[id].BaseCost * (LevelOf(id) + 1);
        public static bool IsMaxed(int id) => LevelOf(id) >= Upgrades[id].MaxLevel;
        public static bool CanBuy(int id) => !IsMaxed(id) && Data.gold >= CostOf(id);

        public static bool Buy(int id)
        {
            if (!CanBuy(id)) return false;
            Data.gold -= CostOf(id);
            Data.upgradeLevels[id]++;
            Save();
            return true;
        }

        // ---- characters ----
        public static bool IsCharacterUnlocked(int idx) => (Data.charUnlockMask & (1 << idx)) != 0;
        public static int SelectedCharacter => Mathf.Clamp(Data.selectedCharacter, 0, Characters.Length - 1);

        public static bool SelectCharacter(int idx)
        {
            if (!IsCharacterUnlocked(idx)) return false;
            Data.selectedCharacter = idx;
            Save();
            return true;
        }

        public static bool UnlockCharacter(int idx)
        {
            if (IsCharacterUnlocked(idx)) return false;
            int cost = Characters[idx].UnlockCost;
            if (Data.gold < cost) return false;
            Data.gold -= cost;
            Data.charUnlockMask |= (1 << idx);
            Save();
            return true;
        }

        // ---- stages ----
        public static int SelectedStage => Mathf.Clamp(Data.selectedStage, 0, Stages.Length - 1);
        public static bool IsStageUnlocked(int idx) => (Data.stageUnlockMask & (1 << idx)) != 0;

        public static bool SelectStage(int idx)
        {
            if (!IsStageUnlocked(idx)) return false;
            Data.selectedStage = idx;
            Save();
            return true;
        }

        public static bool UnlockStage(int idx)
        {
            if (IsStageUnlocked(idx)) return false;
            int cost = Stages[idx].UnlockCost;
            if (Data.gold < cost) return false;
            Data.gold -= cost;
            Data.stageUnlockMask |= (1 << idx);
            Save();
            return true;
        }

        // ---- applied at run start ----
        public static float GoldMult => 1f + 0.10f * LevelOf(2);

        public static void ApplyTo(PlayerStats ps, Health hp)
        {
            if (ps == null) return;
            var ch = Characters[SelectedCharacter];

            ps.DamageMult     += 0.05f * LevelOf(0) + ch.DmgBonus;
            ps.MoveSpeedMult  += 0.03f * LevelOf(3) + ch.SpeedBonus;
            ps.XpMult         += 0.05f * LevelOf(4);
            ps.DamageTakenMult = Mathf.Max(0.2f, ps.DamageTakenMult - 0.03f * LevelOf(5));

            if (hp != null)
            {
                float bonusHp = 15f * LevelOf(1) + ch.HpBonus;
                if (Mathf.Abs(bonusHp) > 0.01f)
                    hp.SetMaxHealth(Mathf.Max(1f, hp.Max + bonusHp), refill: true);
            }
        }
    }
}
