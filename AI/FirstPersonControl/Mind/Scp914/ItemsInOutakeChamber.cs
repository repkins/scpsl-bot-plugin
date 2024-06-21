using InventorySystem.Items.Pickups;
using PluginAPI.Core.Items;
using Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Scp914
{
    internal class ItemsInOutakeChamber : ItemLocations<IItemBeliefCriteria>
    {
        private readonly ItemsWithinSightSense itemsSightSense;

        public ItemsInOutakeChamber(IItemBeliefCriteria criteria, ItemsWithinSightSense itemsSightSense) : base(criteria)
        {
            this.itemsSightSense = itemsSightSense;

            this.itemsSightSense.OnSensedItemWithinSight += OnSensedItemWithinSight;
            this.itemsSightSense.OnAfterSensedItemsWithinSight += OnAfterSensedItemsWithinSight;
        }

        private int numItemsWithinSight = 0;
        private Bounds? outakeChamberBounds;

        private void OnSensedItemWithinSight(ItemPickupBase item)
        {
            if (Criteria.EvaluateItem(item))
            {
                if (!outakeChamberBounds.HasValue)
                {
                    var outakeChamberPosition = Scp914Controller.Singleton.OutputChamber.position;
                    var outakeChamberRotation = Scp914Controller.Singleton.OutputChamber.rotation;
                    var outakeChamberSize = outakeChamberRotation * new Vector3(1.76f, 3.33f, 3.11f);
                    outakeChamberBounds = new Bounds(outakeChamberPosition, outakeChamberSize);
                }

                if (outakeChamberBounds.Value.Contains(item.Position))
                {
                    AddPosition(item.Position);

                    numItemsWithinSight++;
                }
            }
        }

        private readonly HashSet<Vector3> absentPositions = new();

        private void OnAfterSensedItemsWithinSight()
        {
            if (this.Positions.Any() && numItemsWithinSight < this.Positions.Count)
            {
                foreach (var itemPosition in Positions)
                {
                    if (this.itemsSightSense.IsPositionWithinFov(itemPosition)
                        && (!this.itemsSightSense.IsPositionObstructed(itemPosition)))
                    {
                        absentPositions.Add(itemPosition);
                    }
                }
                RemoveAllPositions(absentPositions.Remove);
            }

            numItemsWithinSight = 0;
        }

        public void AddRelativePosition(Vector3 itemRelativePosition)
        {
            var outputChamberPosition = Scp914Controller.Singleton.OutputChamber.position;
            var transformedItemPosition = outputChamberPosition + itemRelativePosition;

            AddPosition(transformedItemPosition);
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"{nameof(ItemsInOutakeChamber)}({this.Criteria}): ");            
            stringBuilder.Append($"{this.Positions.Count}");

            return stringBuilder.ToString();
        }
    }
}
