using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Pickups;
using MapGeneration;
using PluginAPI.Core;
using Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class DoorsWithinSightSense : SightSense
    {
        public HashSet<DoorVariant> DoorsWithinSight { get; } = new();

        public event Action OnBeforeSensedDoorsWithinSight;
        public event Action<DoorVariant> OnSensedDoorWithinSight;
        public event Action OnAfterSensedDoorsWithinSight;

        public DoorsWithinSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        {
        }

        public override void Reset()
        {
            DoorsWithinSight.Clear();
        }

        public override void ProcessSensibility(IEnumerable<Collider> colliders)
        {
            var collidersOfComponent = colliders
                .Select(c => (c, Door: c.GetComponentInParent<DoorVariant>()))
                .Where(t => t.Door is not null && !DoorsWithinSight.Contains(t.Door));

            var withinSight = this.GetWithinSight(collidersOfComponent);

            foreach (var collider in withinSight)
            {
                DoorsWithinSight.Add(collider.GetComponentInParent<DoorVariant>());
            }
        }

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
