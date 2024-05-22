using Interactables;
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
using UnityEngine.Profiling;

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

        private Dictionary<Collider, DoorVariant> collidersOfComponent = new();

        public override void ProcessSensibility(IEnumerable<Collider> colliders)
        {
            Profiler.BeginSample($"{nameof(DoorsWithinSightSense)}.{nameof(ProcessSensibility)}");

            collidersOfComponent.Clear();
            foreach (var collider in colliders)
            {
                if ((doorLayerMask & (1 << collider.gameObject.layer)) != 0 && collider.GetComponentInParent<DoorVariant>() is DoorVariant door)
                {
                    collidersOfComponent.Add(collider, door);
                }
            }

            var withinSight = this.GetWithinSight(collidersOfComponent.Keys);

            foreach (var collider in withinSight)
            {
                DoorsWithinSight.Add(collidersOfComponent[collider]);
            }

            Profiler.EndSample();
        }

        private LayerMask doorLayerMask = LayerMask.GetMask("Door");

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
