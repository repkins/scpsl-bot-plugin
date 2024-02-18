using SCPSLBot.AI.FirstPersonControl.Perception.Senses;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal class ItemOfTypeWithinPickupDistance : ItemWithinPickupDistance<ItemOfType>
    {
        public ItemOfTypeWithinPickupDistance(ItemOfType criteria, ItemsWithinSightSense itemsSightSense) : base(criteria, itemsSightSense)
        {
        }
    }
}
