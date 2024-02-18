using SCPSLBot.AI.FirstPersonControl.Perception.Senses;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal class ItemWithinPickupDistance : ItemOfType<ItemWithinPickupDistanceBase>
    {
        public ItemWithinPickupDistance(ItemType type, ItemsWithinSightSense itemsSightSense) : base(type, itemsSightSense)
        { }
    }
}
