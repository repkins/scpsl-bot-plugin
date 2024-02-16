using Interactables.Interobjects.DoorUtils;
using InventorySystem;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Pickups;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal class KeycardOfPermissions<T> : ItemOfCriteriaBase<T> where T : ItemPickup<ItemPickupBase>
    {
        public KeycardPermissions Permissions;
        public KeycardOfPermissions(KeycardPermissions permissions, ItemsWithinSightSense itemsSightSense, T belief) : base(belief)
        {
            this.Permissions = permissions;

            _itemsSightSense = itemsSightSense;
            _itemsSightSense.OnSensedItemWithinSight += ProcessSensedItem;
            _itemsSightSense.OnAfterSensedItemsWithinSight += ProcessAbsentItem;
        }

        public static implicit operator T(KeycardOfPermissions<T> ofPermissions) => ofPermissions.belief;

        private readonly ItemsWithinSightSense _itemsSightSense;
        private int numItems = 0;

        private void ProcessSensedItem(ItemPickupBase item)
        {
            if (InventoryItemLoader.TryGetItem<KeycardItem>(item.Info.ItemId, out var keycard)
                    && keycard.Permissions.HasFlag(Permissions))
            {
                if (!belief.Item)
                {
                    belief.Update(item);
                }
                numItems++;
            }
        }

        private void ProcessAbsentItem()
        {
            if (numItems > 0)
            {
                if (belief.Item)
                {
                    belief.Update(null);
                }
                numItems = 0;
            }
        }
    }
}
