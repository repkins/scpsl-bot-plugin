using InventorySystem.Items.Pickups;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs
{
    internal class ItemWithinSight<C> : ItemPickup<ItemPickupBase, C> where C : IItemBeliefCriteria
    {
        public ItemWithinSight(C criteria, ItemsWithinSightSense itemsWithinSightSense) : base(criteria, itemsWithinSightSense)
        { }
    }
}
