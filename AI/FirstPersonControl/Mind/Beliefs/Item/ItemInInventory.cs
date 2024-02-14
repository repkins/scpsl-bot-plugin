using InventorySystem.Items;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal class ItemInInventory<T> : ItemInInventory where T : ItemBase
    {
        public ItemInInventory(ItemType itemType) : base(itemType)
        { }

        public new T Item => base.Item as T;
    }

    internal class ItemInInventory : ItemBase, IBelief
    {
        public ItemInInventory(ItemType itemType) : base(itemType)
        { }

        public event Action OnUpdate;
        public ItemBase Item { get; private set; }

        public void Update(ItemBase item)
        {
            Item = item;
            OnUpdate?.Invoke();
        }
    }
}
