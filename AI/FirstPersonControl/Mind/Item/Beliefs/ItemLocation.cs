using InventorySystem.Items.Pickups;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs
{
    internal class ItemLocation<C> : ItemLocation where C : IItemBeliefCriteria
    {
        public C Criteria { get; }
        public ItemLocation(C criteria, ItemsWithinSightSense itemsSightSense) : this(itemsSightSense)
        {
            Criteria = criteria;
        }

        public ItemLocation(ItemsWithinSightSense itemsSightSense)
        {
            this.itemsSightSense = itemsSightSense;
            this.itemsSightSense.OnSensedItemWithinSight += ProcessSensedItem;
            this.itemsSightSense.OnAfterSensedItemsWithinSight += HandleAfterSensedItems;
        }

        private int numItemsWithinSight = 0;

        private void ProcessSensedItem(ItemPickupBase item)
        {
            if (Criteria.EvaluateItem(item))
            {
                Update(item.Position);

                numItemsWithinSight++;
            }
        }

        private void HandleAfterSensedItems()
        {
            if (numItemsWithinSight == 0 && Position.HasValue)
            {
                if (itemsSightSense.IsPositionWithinSight(Position.Value))
                {
                    Update(null);
                }
            }

            numItemsWithinSight = 0;
        }

        private readonly ItemsWithinSightSense itemsSightSense;
    }

    internal class ItemLocation : IBelief
    {
        public Vector3? Position;

        public event Action OnUpdate;

        protected void Update(Vector3? position)
        {
            if (position != Position)
            {
                Position = position;
                OnUpdate?.Invoke();
            }
        }
    }
}
