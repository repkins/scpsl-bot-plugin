using InventorySystem.Items.Pickups;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item
{
    internal class ItemPickup<P, C> : IBelief where P : ItemPickupBase where C : IItemBeliefCriteria
    {
        public C Criteria { get; }
        public ItemPickup(C criteria, ItemsWithinSightSense itemsSightSense)
        {
            Criteria = criteria;

            _itemsSightSense = itemsSightSense;
            _itemsSightSense.OnSensedItemWithinSight += ProcessSensedItem;
            _itemsSightSense.OnAfterSensedItemsWithinSight += ProcessAbsentItem;
        }

        private readonly ItemsWithinSightSense _itemsSightSense;
        private int numItems = 0;

        public ItemPickupBase Item { get; private set; }

        public event Action OnUpdate;

        public void Update(ItemPickupBase value)
        {
            Item = value;
            OnUpdate?.Invoke();
        }

        private void ProcessSensedItem(ItemPickupBase item)
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
    }
}
