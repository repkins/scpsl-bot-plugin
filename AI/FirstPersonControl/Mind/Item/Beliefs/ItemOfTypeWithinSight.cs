using InventorySystem.Items.Pickups;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs
{
    internal class ItemOfTypeWithinSight : ItemWithinSight<ItemOfType>
    {
        public ItemOfTypeWithinSight(ItemOfType criteria, ItemsWithinSightSense itemsSightSense) : base(criteria, itemsSightSense)
        {
        }
    }
}
