using InventorySystem.Items.Keycards;
using InventorySystem.Items.Pickups;
using InventorySystem;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard
{
    internal class KeycardWithinPickupDistance : ItemWithinPickupDistance<KeycardWithPermissions>
    {
        public KeycardWithinPickupDistance(KeycardWithPermissions permissions, ItemsWithinSightSense itemsSightSense) 
            : base(permissions)
        {
            _itemsSightSense = itemsSightSense;
            _itemsSightSense.OnSensedItemWithinSight += ProcessSensedItem;
            _itemsSightSense.OnAfterSensedItemsWithinSight += ProcessAbsentItem;
        }

        private readonly ItemsWithinSightSense _itemsSightSense;
        private int numItems = 0;

        private void ProcessSensedItem(ItemPickupBase item)
        {
            if (InventoryItemLoader.TryGetItem<KeycardItem>(item.Info.ItemId, out var keycard)
                    && keycard.Permissions.HasFlag(Criteria.Permissions))
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
