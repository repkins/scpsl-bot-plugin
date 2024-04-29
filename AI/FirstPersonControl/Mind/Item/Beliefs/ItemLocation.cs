using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs
{
    internal class ItemLocation<C> : IBelief where C : IItemBeliefCriteria
    {
        public C Criteria { get; }

        private readonly FpcBotNavigator navigator;
        private readonly ItemsWithinSightSense itemsSightSense;

        public ItemLocation(C criteria, FpcBotNavigator navigator, ItemsWithinSightSense itemsSightSense)
        {
            this.Criteria = criteria;
            this.navigator = navigator;
            this.itemsSightSense = itemsSightSense;
            this.itemsSightSense.OnAfterSensedItemsWithinSight += this.OnAfterSensedItemsWithinSight;
        }

        private void OnAfterSensedItemsWithinSight()
        {
            this.EvaluateSetAccesablePosition();
        }

        private readonly HashSet<Vector3> inaccessiblePositions = new();

        protected bool IsAccessible(Vector3 position)
        {
            var pathOfPoints = this.navigator.PointsPath;
            var goalPosition = pathOfPoints.Last();

            if (goalPosition != position)
            {
                return !this.inaccessiblePositions.Contains(position);
            }

            var pathSegments = pathOfPoints.Zip(pathOfPoints.Skip(1), (point, nextPoint) => (point, nextPoint));

            var hits = pathSegments.Select(segment => (isHit: Physics.Linecast(segment.point, segment.nextPoint, out var hit), hit))
                .Where(t => t.isHit);

            if (hits.All(t => this.Criteria.CanOvercome(t.hit.collider)))
            {
                return true;
            }

            Log.Debug($"{this}: cannot overcome");

            return false;
        }

        public Vector3? AccessiblePosition;

        public event Action OnUpdate;

        protected void SetAccesablePosition(Vector3 newPosition)
        {
            if (newPosition != this.AccessiblePosition)
            {
                this.inaccessiblePositions.Remove(newPosition);

                this.AccessiblePosition = newPosition;
                this.OnUpdate?.Invoke();
            }
        }

        protected void ClearPosition()
        {
            this.AccessiblePosition = null;
            this.OnUpdate?.Invoke();
        }

        private void EvaluateSetAccesablePosition()
        {
            if (this.AccessiblePosition.HasValue && !this.IsAccessible(AccessiblePosition.Value))
            {
                this.inaccessiblePositions.Add(this.AccessiblePosition.Value);

                ClearPosition();
            }
        }
    }
}
