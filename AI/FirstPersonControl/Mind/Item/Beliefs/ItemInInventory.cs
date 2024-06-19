using InventorySystem.Items;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs
{
    internal class ItemInInventory<C> : Belief<bool> where C : IItemBeliefCriteria
    {
        public C Criteria { get; }
        public ItemInInventory(C criteria, ItemsInInventorySense inventorySense)
        {
            Criteria = criteria;
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

        public ItemBase Item { get; private set; }

        public void Update(ItemBase item)
        {
            Item = item;
            InvokeOnUpdate();
        }

        public override string ToString()
        {
            return $"{nameof(ItemInInventory<C>)}({Criteria}): {Item}";
        }
    }
}
