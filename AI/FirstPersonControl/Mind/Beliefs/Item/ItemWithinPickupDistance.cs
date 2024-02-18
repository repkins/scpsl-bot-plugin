using InventorySystem.Items.Pickups;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal class ItemWithinPickupDistance<C> : ItemPickup<ItemPickupBase, C> where C : IItemBeliefCriteria
    {
        public ItemWithinPickupDistance(C criteria, ItemsWithinSightSense itemsSightSense) : base(criteria, itemsSightSense)
        {
        }
    }
}
