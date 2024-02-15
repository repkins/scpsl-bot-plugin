using InventorySystem.Items.Pickups;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal class ItemWithinPickupDistance : ItemWithinPickupDistance<ItemPickupBase>
    {
        public ItemType ItemType { get; }
        public ItemWithinPickupDistance(ItemType itemType)
        {
            ItemType = itemType;
        }
    }
}
