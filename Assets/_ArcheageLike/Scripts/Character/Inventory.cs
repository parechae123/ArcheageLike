using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using ArcheageLike.Data;
using ArcheageLike.Core;

namespace ArcheageLike.Character
{
    /// <summary>
    /// Inventory system with item stacking.
    /// ArcheAge style with weight limit and categorized slots.
    /// </summary>
    public class Inventory : MonoBehaviour
    {
        [System.Serializable]
        public class ItemSlot
        {
            public ItemData item;
            public int amount;

            public bool IsEmpty => item == null || amount <= 0;
        }

        [Header("Settings")]
        [SerializeField] private int _maxSlots = 50;
        [SerializeField] private float _maxWeight = 100f;

        private List<ItemSlot> _slots = new List<ItemSlot>();
        private float _currentWeight;
        private int _gold;

        public int MaxSlots => _maxSlots;
        public float MaxWeight => _maxWeight;
        public float CurrentWeight => _currentWeight;
        public int Gold => _gold;
        public List<ItemSlot> Slots => _slots;

        public UnityEvent OnInventoryChanged = new UnityEvent();

        private void Awake()
        {
            for (int i = 0; i < _maxSlots; i++)
                _slots.Add(new ItemSlot());
        }

        public bool AddItem(ItemData item, int amount = 1)
        {
            if (item == null || amount <= 0) return false;

            float addedWeight = item.weight * amount;
            if (_currentWeight + addedWeight > _maxWeight)
            {
                Debug.Log("[Inventory] Too heavy!");
                return false;
            }

            // Try stack existing
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].item == item && _slots[i].amount < item.maxStack)
                {
                    int canAdd = Mathf.Min(amount, item.maxStack - _slots[i].amount);
                    _slots[i].amount += canAdd;
                    amount -= canAdd;
                    _currentWeight += item.weight * canAdd;

                    if (amount <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        EventBus.Publish(new ItemPickedUpEvent { ItemId = item.itemId, Amount = canAdd });
                        return true;
                    }
                }
            }

            // Find empty slot for remaining
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsEmpty)
                {
                    int canAdd = Mathf.Min(amount, item.maxStack);
                    _slots[i].item = item;
                    _slots[i].amount = canAdd;
                    _currentWeight += item.weight * canAdd;
                    amount -= canAdd;

                    if (amount <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }

            Debug.Log("[Inventory] Not enough space!");
            OnInventoryChanged?.Invoke();
            return amount <= 0;
        }

        public bool RemoveItem(ItemData item, int amount = 1)
        {
            int remaining = amount;
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].item == item)
                {
                    int canRemove = Mathf.Min(remaining, _slots[i].amount);
                    _slots[i].amount -= canRemove;
                    _currentWeight -= item.weight * canRemove;
                    remaining -= canRemove;

                    if (_slots[i].amount <= 0)
                    {
                        _slots[i].item = null;
                        _slots[i].amount = 0;
                    }

                    if (remaining <= 0) break;
                }
            }

            OnInventoryChanged?.Invoke();
            return remaining <= 0;
        }

        public bool HasItem(ItemData item, int amount = 1)
        {
            int total = 0;
            foreach (var slot in _slots)
            {
                if (slot.item == item)
                    total += slot.amount;
            }
            return total >= amount;
        }

        public int GetItemCount(ItemData item)
        {
            int total = 0;
            foreach (var slot in _slots)
            {
                if (slot.item == item)
                    total += slot.amount;
            }
            return total;
        }

        public void AddGold(int amount)
        {
            _gold += amount;
            OnInventoryChanged?.Invoke();
        }

        public bool SpendGold(int amount)
        {
            if (_gold < amount) return false;
            _gold -= amount;
            OnInventoryChanged?.Invoke();
            return true;
        }
    }
}
