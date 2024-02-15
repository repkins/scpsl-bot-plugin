using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using InventorySystem.Items.Keycards;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard
{
    internal class KeycardInInventory : ItemInInventory<KeycardItem>
    {
        public KeycardPermissions Permissions;
        public KeycardInInventory(KeycardPermissions permissions, ItemsInInventorySense itemsInInventorySense)
        {
            this.Permissions = permissions;

            _itemsInInventorySense = itemsInInventorySense;
            _itemsInInventorySense.OnSensedItem += ProcessSensedItem;
            _itemsInInventorySense.OnAfterSensedItems += ProcessAbsentItem;
        }

        private readonly ItemsInInventorySense _itemsInInventorySense;
        private int numItems = 0;

        private void ProcessSensedItem(ItemBase item)
        {
            if (item is KeycardItem keycard && keycard.Permissions.HasFlag(Permissions))
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
