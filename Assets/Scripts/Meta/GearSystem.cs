using System;
using UnityEngine;

namespace SurvivorIO
{
    [Serializable]
    public class GearItem
    {
        public int slot;
        public int rarity;
        public int level = 1;
    }

    /// <summary>
    /// Equipment meta-system (Survivor.io style): 8 gear slots, 5 rarities,
    /// per-piece leveling, a gacha chest and equipped-stat bonuses applied at run
    /// start. Inventory + equipped indices persist in the shared SaveData.
    /// </summary>
    public static class GearSystem
    {
        public const int SlotCount = 8;
        public const int RarityCount = 5;
        public const int ChestCost = 100;

        public static readonly string[] SlotNames =
            { "Weapon", "Armor", "Gloves", "Boots", "Belt", "Necklace", "Bracelet", "Ring" };
        public static readonly string[] RarityNames =
            { "Common", "Great", "Rare", "Epic", "Legendary" };
        public static readonly Color[] RarityColors =
        {
            new Color(0.7f, 0.7f, 0.7f),   // Common  grey
            new Color(0.4f, 0.85f, 0.4f),  // Great   green
            new Color(0.35f, 0.6f, 1f),    // Rare    blue
            new Color(0.75f, 0.45f, 1f),   // Epic    purple
            new Color(1f, 0.7f, 0.2f),     // Legend  gold
        };

        private static readonly float[] RarityMult = { 1f, 1.6f, 2.4f, 3.5f, 5f };

        // stat kind per slot: 0 dmg%, 1 HP, 2 cooldown%, 3 move%, 4 pickup%, 5 xp%
        private static readonly int[] SlotStat   = { 0, 1, 2, 3, 1, 0, 4, 5 };
        private static readonly float[] SlotBase  = { 0.05f, 15f, 0.03f, 0.03f, 12f, 0.04f, 0.10f, 0.05f };

        // ---- save access ----
        private static SaveData D => MetaProgress.Data;

        public static System.Collections.Generic.List<GearItem> Inventory
        {
            get
            {
                if (D.gearInventory == null) D.gearInventory = new System.Collections.Generic.List<GearItem>();
                return D.gearInventory;
            }
        }

        public static int[] Equipped
        {
            get
            {
                if (D.gearEquipped == null || D.gearEquipped.Length != SlotCount)
                {
                    D.gearEquipped = new int[SlotCount];
                    for (int i = 0; i < SlotCount; i++) D.gearEquipped[i] = -1;
                }
                return D.gearEquipped;
            }
        }

        // ---- queries ----
        public static GearItem EquippedIn(int slot)
        {
            int idx = Equipped[slot];
            return (idx >= 0 && idx < Inventory.Count) ? Inventory[idx] : null;
        }

        public static float StatValue(GearItem g)
        {
            return SlotBase[g.slot] * RarityMult[g.rarity] * (1f + 0.15f * (g.level - 1));
        }

        public static int StatKind(GearItem g) => SlotStat[g.slot];

        public static string StatLabel(GearItem g)
        {
            float v = StatValue(g);
            switch (SlotStat[g.slot])
            {
                case 0: return $"+{Mathf.RoundToInt(v * 100)}% damage";
                case 1: return $"+{Mathf.RoundToInt(v)} HP";
                case 2: return $"-{Mathf.RoundToInt(v * 100)}% cooldown";
                case 3: return $"+{Mathf.RoundToInt(v * 100)}% move speed";
                case 4: return $"+{Mathf.RoundToInt(v * 100)}% pickup";
                default: return $"+{Mathf.RoundToInt(v * 100)}% XP";
            }
        }

        public static int UpgradeCost(GearItem g) => 30 * g.level * (g.rarity + 1);

        // ---- actions ----
        public static GearItem RollChest()
        {
            if (MetaProgress.Gold < ChestCost) return null;
            MetaProgress.Data.gold -= ChestCost;

            var item = new GearItem
            {
                slot = UnityEngine.Random.Range(0, SlotCount),
                rarity = RollRarity(),
                level = 1
            };
            Inventory.Add(item);

            // Auto-equip if the slot is empty or this is strictly better.
            int slotIdx = item.slot;
            var current = EquippedIn(slotIdx);
            if (current == null || StatValue(item) > StatValue(current))
                Equipped[slotIdx] = Inventory.Count - 1;

            MetaProgress.Save();
            return item;
        }

        public static bool Equip(int inventoryIndex)
        {
            if (inventoryIndex < 0 || inventoryIndex >= Inventory.Count) return false;
            var item = Inventory[inventoryIndex];
            Equipped[item.slot] = inventoryIndex;
            MetaProgress.Save();
            return true;
        }

        public static bool Upgrade(GearItem g)
        {
            if (g == null) return false;
            int cost = UpgradeCost(g);
            if (MetaProgress.Gold < cost) return false;
            MetaProgress.Data.gold -= cost;
            g.level++;
            MetaProgress.Save();
            return true;
        }

        private static int RollRarity()
        {
            float r = UnityEngine.Random.value;
            if (r < 0.50f) return 0;   // Common
            if (r < 0.78f) return 1;   // Great
            if (r < 0.93f) return 2;   // Rare
            if (r < 0.99f) return 3;   // Epic
            return 4;                  // Legendary
        }

        // ---- applied at run start ----
        public static void ApplyTo(PlayerStats ps, Health hp)
        {
            if (ps == null) return;
            float dmg = 0f, cd = 0f, move = 0f, pickup = 0f, xp = 0f, addHp = 0f;

            for (int slot = 0; slot < SlotCount; slot++)
            {
                var g = EquippedIn(slot);
                if (g == null) continue;
                float v = StatValue(g);
                switch (SlotStat[slot])
                {
                    case 0: dmg += v; break;
                    case 1: addHp += v; break;
                    case 2: cd += v; break;
                    case 3: move += v; break;
                    case 4: pickup += v; break;
                    case 5: xp += v; break;
                }
            }

            ps.DamageMult     += dmg;
            ps.MoveSpeedMult  += move;
            ps.PickupRangeMult += pickup;
            ps.XpMult         += xp;
            ps.CooldownMult   *= Mathf.Clamp(1f - cd, 0.2f, 1f);
            if (hp != null && addHp > 0f)
                hp.SetMaxHealth(hp.Max + addHp, refill: true);
        }
    }
}
