using Interactables;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Pickups;
using MapGeneration;
using MapGeneration.Distributors;
using PluginAPI.Core;
using Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
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

        private static Dictionary<Collider, DoorVariant> allCollidersToComponent = new();

        private Dictionary<Collider, DoorVariant> validCollidersToComponent = new();

        public override IEnumerator<JobHandle> ProcessSensibility(IEnumerable<Collider> colliders)
        {
            Profiler.BeginSample($"{nameof(DoorsWithinSightSense)}.{nameof(ProcessSensibility)}");

            validCollidersToComponent.Clear();
            foreach (var collider in colliders)
            {
                if ((doorLayerMask & (1 << collider.gameObject.layer)) != 0)
                {
                    if (!allCollidersToComponent.TryGetValue(collider, out var component))
                    {
                        component = collider.GetComponentInParent<DoorVariant>();
                        allCollidersToComponent.Add(collider, component);
                    }

                    if (component != null)
                    {
                        validCollidersToComponent.Add(collider, component);
                    }
                }
            }


            var withinSight = new List<Collider>();
            var withinSightHandles = this.GetWithinSight(validCollidersToComponent.Keys, withinSight);
            while (withinSightHandles.MoveNext())
            {
                yield return withinSightHandles.Current;
            }


            foreach (var collider in withinSight)
            {
                DoorsWithinSight.Add(validCollidersToComponent[collider]);
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
