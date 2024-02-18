using InventorySystem.Items.Pickups;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal class ItemOfTypeWithinSight : ItemWithinSight<ItemOfType>
    {
        public ItemOfTypeWithinSight(ItemOfType criteria, ItemsWithinSightSense itemsSightSense) : base(criteria)
        {
            _itemsSightSense = itemsSightSense;
            _itemsSightSense.OnSensedItemWithinSight += ProcessSensedItem;
            _itemsSightSense.OnAfterSensedItemsWithinSight += ProcessAbsentItem;
        }

        private readonly ItemsWithinSightSense _itemsSightSense;
        private int numItems = 0;

        private void ProcessSensedItem(ItemPickupBase item)
        {
            if (item.Info.ItemId == Criteria.ItemType)
            {
                if (!Item)
                {
                    Update(item);
                }
                numItems++;
            }
        }

        private void ProcessAbsentItem()
        {
            if (numItems > 0)
            {
                if (Item)
                {
                    Update(null);
                }
                numItems = 0;
            }
        }
    }
}
