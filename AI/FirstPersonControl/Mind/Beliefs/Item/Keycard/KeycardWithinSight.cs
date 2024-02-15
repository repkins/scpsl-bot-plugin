using Interactables.Interobjects.DoorUtils;
using InventorySystem;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Pickups;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard
{
    internal class KeycardWithinSight : ItemWithinSight<KeycardPickup>
    {
        public KeycardPermissions Permissions;
        public KeycardWithinSight(KeycardPermissions permissions, ItemsWithinSightSense itemsSightSense)
        {
            this.Permissions = permissions;

            _itemsSightSense = itemsSightSense;
            _itemsSightSense.OnSensedItemWithinSight += ProcessSensedItem;
            _itemsSightSense.OnAfterSensedItemsWithinSight += ProcessAbsentItem;
        }

        private readonly ItemsWithinSightSense _itemsSightSense;
        private int numItems = 0;

        private void ProcessSensedItem(ItemPickupBase item)
        {
            if (InventoryItemLoader.TryGetItem<KeycardItem>(item.Info.ItemId, out var keycard)
                    && keycard.Permissions.HasFlag(Permissions))
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
