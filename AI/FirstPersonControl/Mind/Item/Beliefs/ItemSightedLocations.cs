using InventorySystem.Items.Pickups;
using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs
{
    internal partial class ItemSightedLocations<C> : ItemLocations<C> where C : IItemBeliefCriteria
    {
        private readonly ItemsWithinSightSense itemsSightSense;

        public ItemSightedLocations(C criteria, ItemsWithinSightSense itemsSightSense) : base(criteria)
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
                AddPosition(item.Position);

                numItemsWithinSight++;
            }
        }

        private void HandleAfterSensedItems()
        {
            // Evaluate item positions out of sight
            if (numItemsWithinSight < Positions.Count)
            {
                foreach (var sightedPosition in Positions)
                {
                    if (itemsSightSense.IsPositionWithinFov(sightedPosition)
                        && (!itemsSightSense.IsPositionObstructed(sightedPosition) || itemsSightSense.GetDistanceToPosition(sightedPosition) < 1.5f))
                    {
                        RemovePosition(sightedPosition);
                    }
                }
            }
            numItemsWithinSight = 0;
        }

        public override string ToString()
        {
            return $"{nameof(ItemSightedLocations<C>)}({this.Criteria}): {this.Positions.Count}";
        }
    }
}
