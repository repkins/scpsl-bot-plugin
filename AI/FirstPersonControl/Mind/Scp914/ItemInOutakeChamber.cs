using InventorySystem.Items.Pickups;
using Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Item;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Scp914
{
    internal class ItemInOutakeChamber : IBelief
    {
        public IItemBeliefCriteria Criteria { get; }

        private readonly ItemsWithinSightSense itemsSightSense;

        public ItemInOutakeChamber(IItemBeliefCriteria criteria, ItemsWithinSightSense itemsSightSense)
        {
            this.Criteria = criteria;
            this.itemsSightSense = itemsSightSense;

            this.itemsSightSense.OnSensedItemWithinSight += OnSensedItemWithinSight;
            this.itemsSightSense.OnAfterSensedItemsWithinSight += OnAfterSensedItemsWithinSight;
        }

        private int numItemsWithinSight = 0;

        private void OnSensedItemWithinSight(ItemPickupBase item)
        {
            if (Criteria.EvaluateItem(item))
            {
                var outakeChamberPosition = Scp914Controller.Singleton.OutputChamber.position;
                var outakeChamberRotation = Scp914Controller.Singleton.OutputChamber.rotation;
                var outakeChamberSize = outakeChamberRotation * new Vector3(1.76f, 3.33f, 3.11f);
                var outakeChamberBounds = new Bounds(outakeChamberPosition, outakeChamberSize);

                if (outakeChamberBounds.Contains(item.Position))
                {
                    var relPosition = item.Position - outakeChamberPosition;
                    Update(relPosition);

                    numItemsWithinSight++;
                }
            }
        }

        private void OnAfterSensedItemsWithinSight()
        {
            if (numItemsWithinSight == 0 && this.PositionRelative.HasValue)
            {
                var outakeChamberPosition = Scp914Controller.Singleton.OutputChamber.position;

                var itemPosition = outakeChamberPosition + this.PositionRelative.Value;

                if (this.itemsSightSense.IsPositionWithinFov(itemPosition)
                    && (!this.itemsSightSense.IsPositionObstructed(itemPosition)))
                {
                    Update(null);
                }
            }

            numItemsWithinSight = 0;
        }

        public Vector3? PositionRelative { get; private set; }
        public event Action OnUpdate;

        public void Update(Vector3? newRelativePosition)
        {
            if (newRelativePosition != this.PositionRelative)
            {
                this.PositionRelative = newRelativePosition;
                this.OnUpdate?.Invoke();
            }
        }

        public override string ToString()
        {
            return $"{nameof(ItemInOutakeChamber)}({this.Criteria}, {this.PositionRelative})";
        }
    }
}
