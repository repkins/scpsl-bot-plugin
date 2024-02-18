using SCPSLBot.AI.FirstPersonControl.Perception.Senses;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal class ItemWithinSight : ItemOfType<ItemWithinSightBase>
    {
        public ItemWithinSight(ItemType type, ItemsWithinSightSense itemsSightSense) : base(type, itemsSightSense)
        {
        }
    }
}
