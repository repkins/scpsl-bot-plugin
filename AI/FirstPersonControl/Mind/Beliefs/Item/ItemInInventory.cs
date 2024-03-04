using InventorySystem.Items;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal class ItemInInventory<C> : ItemInInventory<C, ItemBase> where C : IItemBeliefCriteria
    {
        public ItemInInventory(C criteria, ItemsInInventorySense inventorySense) : base(criteria, inventorySense)
        {
        }
    }

    internal class ItemInInventory<C, T> : ItemInInventoryBase where C : IItemBeliefCriteria where T : ItemBase
    {
        public readonly C Criteria;
        public ItemInInventory(C criteria, ItemsInInventorySense inventorySense)
        {
            this.Criteria = criteria;
            this._itemsInInventorySense = inventorySense;

            _itemsInInventorySense.OnSensedItem += ProcessSensedItem;
            _itemsInInventorySense.OnAfterSensedItems += ProcessAbsentItem;
        }

        private readonly ItemsInInventorySense _itemsInInventorySense;
        private int numItems = 0;

        private void ProcessSensedItem(ItemBase item)
        {
            if (Criteria.EvaluateItem(item))
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
            if (numItems <= 0)
            {
                if (Item)
                {
                    Update(null);
                }
            }

            numItems = 0;
        }

        public override string ToString()
        {
            return $"{GetType().Name}({Criteria})";
        }

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
