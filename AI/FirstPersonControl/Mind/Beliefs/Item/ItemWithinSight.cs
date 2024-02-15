using InventorySystem.Items.Pickups;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal class ItemWithinSight : ItemWithinSight<ItemPickupBase>
    {
        public ItemType ItemType { get; }
        public ItemWithinSight(ItemType itemType)
        {
            ItemType = itemType;
        }
    }
}
