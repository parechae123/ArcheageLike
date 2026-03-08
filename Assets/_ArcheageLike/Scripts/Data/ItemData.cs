using UnityEngine;

namespace ArcheageLike.Data
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "ArcheageLike/Item Data")]
    public class ItemData : ScriptableObject
    {
        public int itemId;
        public string itemName;
        [TextArea] public string description;
        public Sprite icon;
        public ItemType itemType;
        public ItemRarity rarity;
        public int maxStack = 99;
        public float weight = 1f;

        [Header("Consumable")]
        public float healAmount;
        public float manaRestoreAmount;

        [Header("Equipment")]
        public float attackBonus;
        public float defenseBonus;
        public EquipSlot equipSlot;

        [Header("Trade")]
        public int buyPrice;
        public int sellPrice;
        public bool isTradePack;

        [Header("Farming")]
        public bool isSeed;
        public float growTime = 60f;
        public ItemData harvestResult;
        public int harvestAmount = 1;
    }

    public enum ItemType
    {
        Consumable,
        Equipment,
        Material,
        QuestItem,
        TradePack,
        Seed,
        Crop
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Heroic,
        Unique,
        Legendary,
        Mythic
    }

    public enum EquipSlot
    {
        None,
        Head,
        Chest,
        Legs,
        Feet,
        Hands,
        MainHand,
        OffHand,
        Accessory
    }
}
