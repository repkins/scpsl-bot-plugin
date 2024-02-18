using InventorySystem.Items;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal class ItemInInventory<T> : ItemInInventoryBase where T : ItemBase
    {
        public ItemInInventory()
        { }

        public new T Item => base.Item as T;
    }

    internal class ItemInInventoryBase : IBelief
    {
        public event Action OnUpdate;
        public ItemBase Item { get; private set; }

        public void Update(ItemBase item)
        {
            Item = item;
            OnUpdate?.Invoke();
        }
    }
}
