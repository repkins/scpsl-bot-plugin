using Interactables.Interobjects.DoorUtils;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class DoorsWithinSightSense : MoveableWithinSightSense<DoorVariant>
    {
        public HashSet<DoorVariant> DoorsWithinSight => ComponentsWithinSight;

        public event Action OnBeforeSensedDoorsWithinSight;
        public event Action<DoorVariant> OnSensedDoorWithinSight;
        public event Action OnAfterSensedDoorsWithinSight;

        public DoorsWithinSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        { }

        private LayerMask doorLayerMask = LayerMask.GetMask("Door");
        protected override LayerMask LayerMask => doorLayerMask;

        public override void ProcessSightSensedItems()
        {
            OnBeforeSensedDoorsWithinSight?.Invoke();
            foreach (var doorWithinSight in DoorsWithinSight)
            {
                OnSensedDoorWithinSight?.Invoke(doorWithinSight);
            }
            OnAfterSensedDoorsWithinSight?.Invoke();
        }
    }
}
