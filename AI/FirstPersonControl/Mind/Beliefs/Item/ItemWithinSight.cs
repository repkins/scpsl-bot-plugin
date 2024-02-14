using InventorySystem.Items.Pickups;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal class ItemWithinSight : ItemPickup<ItemPickupBase>
    {
        public ItemWithinSight(ItemType itemType) : base(itemType)
        { }
    }
}
