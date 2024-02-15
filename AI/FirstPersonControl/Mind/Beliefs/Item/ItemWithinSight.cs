using InventorySystem.Items.Pickups;
using PluginAPI.Core.Items;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal class ItemWithinSight : ItemWithinSight<ItemPickupBase>
    {
        public ItemType ItemType { get; }
        public ItemWithinSight(ItemType itemType, ItemsWithinSightSense itemsSightSense)
        {
            ItemType = itemType;

            _itemsSightSense = itemsSightSense;
            _itemsSightSense.OnSensedItemWithinSight += ProcessSensedItem;
            _itemsSightSense.OnAfterSensedItemsWithinSight += ProcessAbsentItem;
        }

        private readonly ItemsWithinSightSense _itemsSightSense;
        private int numItems = 0;

        private void ProcessSensedItem(ItemPickupBase itemPickup)
        {
            if (itemPickup.Info.ItemId == ItemType)
            {
                if (!Item)
                {
                    Update(itemPickup);
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
