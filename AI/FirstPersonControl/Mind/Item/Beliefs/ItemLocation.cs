using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs
{
    internal class ItemLocation<C> : Belief<bool> where C : IItemBeliefCriteria
    {
        public C Criteria { get; }
        private ItemLocation(C criteria)
        {
            this.Criteria = criteria;
        }

        private readonly DoorObstacle doorObstacle;
        public ItemLocation(C criteria, DoorObstacle doorObstacle) : this(criteria)
        {
            this.doorObstacle = doorObstacle;
            this.doorObstacle.OnUpdate += OnObstaclesUpdated;
        }

        private void OnObstaclesUpdated()
        {
            this.EvaluateSetAccesablePosition();
        }

        private readonly HashSet<Vector3> inaccessiblePositions = new();

        protected bool IsAccessible(Vector3 position)
        {
            if (this.Criteria.CanReach(position, this.doorObstacle))
            {
                return true;
            }

            return false;
        }

        public Vector3? AccessiblePosition;

        protected void SetAccesablePosition(Vector3 newPosition)
        {
            if (newPosition != this.AccessiblePosition)
            {
                this.inaccessiblePositions.Remove(newPosition);

                this.AccessiblePosition = newPosition;
                this.InvokeOnUpdate();
            }
        }

        protected void ClearPosition()
        {
            this.AccessiblePosition = null;
            this.InvokeOnUpdate();
        }

        private void EvaluateSetAccesablePosition()
        {
            if (this.AccessiblePosition.HasValue && !this.IsAccessible(AccessiblePosition.Value))
            {
                this.inaccessiblePositions.Add(this.AccessiblePosition.Value);

                ClearPosition();

                Log.Debug($"{this}: not accessable anymore");
            }
        }
    }
}
