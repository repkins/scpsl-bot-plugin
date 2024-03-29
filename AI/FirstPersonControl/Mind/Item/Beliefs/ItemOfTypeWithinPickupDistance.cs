using SCPSLBot.AI.FirstPersonControl.Perception.Senses;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs
{
    internal class ItemOfTypeWithinPickupDistance : ItemWithinPickupDistance<ItemOfType>
    {
        public ItemOfTypeWithinPickupDistance(ItemOfType criteria, ItemsWithinSightSense itemsSightSense) : base(criteria, itemsSightSense)
        {
        }
    }
}
