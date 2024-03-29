using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs
{
    internal struct ItemOfType : IItemBeliefCriteria, IEquatable<ItemOfType>
    {
        public ItemType ItemType;
        public ItemOfType(ItemType type)
        {
            this.ItemType = type;
        }

        public bool EvaluateItem(ItemPickupBase item)
        {
            return item.Info.ItemId == ItemType;
        }

        public bool EvaluateItem(ItemBase item)
        {
            return item.ItemTypeId == ItemType;
        }

        public bool Matches(ItemType inItemType)
        {
            return inItemType == this.ItemType;
        }

        public bool Equals(IItemBeliefCriteria other)
        {
            return other is ItemOfType otherOf && Equals(otherOf);
        }

        public bool Equals(ItemOfType other)
        {
            return other.ItemType == this.ItemType;
        }

        public static implicit operator ItemOfType(ItemType itemType) => new(itemType);

        public override string ToString()
        {
            return $"{ItemType}";
        }
    }
}
