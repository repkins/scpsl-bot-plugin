using InventorySystem.Items.Pickups;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal class ItemWithinPickupDistance : ItemPickup<ItemPickupBase>
    {
        public ItemWithinPickupDistance(ItemType itemType) : base(itemType)
        { }
    }
}
