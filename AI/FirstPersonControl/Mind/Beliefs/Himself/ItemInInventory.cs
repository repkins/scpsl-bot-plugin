using InventorySystem.Items;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Himself
{
    internal class ItemInInventory<T> : ItemInInventory where T : ItemBase
    {
        public new T Item => base.Item as T;
    }

    internal class ItemInInventory : IBelief
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
