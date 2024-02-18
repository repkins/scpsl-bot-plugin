using InventorySystem.Items.Pickups;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal class ItemWithinPickupDistance<C> : ItemPickup<ItemPickupBase, C> where C : struct
    {
        public ItemWithinPickupDistance(C criteria) : base(criteria)
        {
        }
    }
}
