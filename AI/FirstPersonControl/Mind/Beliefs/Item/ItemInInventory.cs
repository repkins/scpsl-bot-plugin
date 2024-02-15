using InventorySystem.Items;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal class ItemInInventory : ItemInInventory<ItemBase>
    {
        public ItemType ItemType { get; }
        public ItemInInventory(ItemType itemType, ItemsInInventorySense itemsInInventorySense)
        {
            ItemType = itemType;

            _itemsInInventorySense = itemsInInventorySense;
            _itemsInInventorySense.OnSensedItem += ProcessSensedItem;
            _itemsInInventorySense.OnAfterSensedItems += ProcessAbsentItem;
        }

        private readonly ItemsInInventorySense _itemsInInventorySense;
        private int numItems = 0;

        private void ProcessSensedItem(ItemBase item)
        {
            if (item.ItemTypeId == ItemType)
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
