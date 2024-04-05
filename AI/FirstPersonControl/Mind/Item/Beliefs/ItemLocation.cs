using InventorySystem.Items.Pickups;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs
{
    internal partial class ItemLocation<C>
    {
        public ItemLocation(C criteria, ItemsWithinSightSense itemsSightSense) : this(criteria)
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
                if (itemsSightSense.IsPositionWithinFov(Position.Value) 
                    && (!itemsSightSense.IsPositionObstructed(Position.Value) || itemsSightSense.GetDistanceToPosition(Position.Value) < 1.5f))
                {
                    Update(null);
                }
            }

            numItemsWithinSight = 0;
        }

        private readonly ItemsWithinSightSense itemsSightSense;
    }

    internal partial class ItemLocation<C> : ItemLocation where C : IItemBeliefCriteria
    {
        public C Criteria { get; }
        public ItemLocation(C criteria)
        {
            Criteria = criteria;
        }
    }

    internal class ItemLocation : IBelief
    {
        public Vector3? Position;
        public bool IsKnown => Position.HasValue;

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
