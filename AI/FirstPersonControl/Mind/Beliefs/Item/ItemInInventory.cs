using InventorySystem.Items;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal class ItemInInventory : ItemInInventory<ItemBase>
    {
        public ItemType ItemType { get; }
        public ItemInInventory(ItemType itemType)
        {
            ItemType = itemType;
        }
    }
}
