﻿using InventorySystem.Items.Pickups;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs
{
    internal partial class ItemSightedLocation<C> : ItemLocation<C> where C : IItemBeliefCriteria
    {
        private readonly ItemsWithinSightSense itemsSightSense;

        public ItemSightedLocation(C criteria, FpcBotNavigator navigator, ItemsWithinSightSense itemsSightSense) : base(criteria, navigator, itemsSightSense)
        {
            this.itemsSightSense = itemsSightSense;
            this.itemsSightSense.OnSensedItemWithinSight += ProcessSensedItem;
            this.itemsSightSense.OnAfterSensedItemsWithinSight += HandleAfterSensedItems;
        }

        private int numItemsWithinSight = 0;

        private void ProcessSensedItem(ItemPickupBase item)
        {
            if (Criteria.EvaluateItem(item) && IsAccessible(item.Position))
            {
                SetAccesablePosition(item.Position);

                numItemsWithinSight++;
            }
        }

        private void HandleAfterSensedItems()
        {
            // Evaluate item position out of sight
            if (Position.HasValue && numItemsWithinSight == 0)
            {
                if (itemsSightSense.IsPositionWithinFov(Position.Value)
                    && (!itemsSightSense.IsPositionObstructed(Position.Value) || itemsSightSense.GetDistanceToPosition(Position.Value) < 1.5f))
                {
                    ClearPosition();
                }
            }
            numItemsWithinSight = 0;
        }

        public override string ToString()
        {
            return $"{nameof(ItemSightedLocation<C>)}({this.Criteria})";
        }
    }
}
