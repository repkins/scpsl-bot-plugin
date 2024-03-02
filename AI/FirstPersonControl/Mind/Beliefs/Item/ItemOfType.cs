using InventorySystem.Items.Pickups;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal struct ItemOfType : IItemBeliefCriteria
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

        public bool Equals(IItemBeliefCriteria other)
        {
            return other is ItemOfType otherOf && otherOf.ItemType == this.ItemType;
        }

        public override string ToString()
        {
            return $"{ItemType}";
        }
    }
}
