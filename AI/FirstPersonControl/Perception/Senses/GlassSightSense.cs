﻿using Interactables.Interobjects.DoorUtils;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class GlassSightSense : SightSense, ISense
    {
        public HashSet<BreakableWindow> WindowsWithinSight { get; } = new();

        public event Action<BreakableWindow> OnSensedWindowsWithinSight;
        public event Action OnAfterSensedWindowsWithinSight;

        public GlassSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        {
        }

        public override void Reset()
        {
            WindowsWithinSight.Clear();
        }

        private Dictionary<Collider, BreakableWindow> collidersOfComponent = new();

        public override IEnumerator<JobHandle> ProcessSensibility(IEnumerable<Collider> colliders)
        {
            Profiler.BeginSample($"{nameof(GlassSightSense)}.{nameof(ProcessSensibility)}");

            collidersOfComponent.Clear();

            foreach (var collider in colliders)
            {
                if ((glassLayerMask & (1 << collider.gameObject.layer)) != 0 && collider.GetComponentInParent<BreakableWindow>() is BreakableWindow window)
                {
                    collidersOfComponent.Add(collider, window);
                }
            }


            var withinSight = new List<Collider>();
            var withinSightHandles = this.GetWithinSight(collidersOfComponent.Keys, withinSight);
            while (withinSightHandles.MoveNext())
            {
                yield return withinSightHandles.Current;
            }


            foreach (var collider in withinSight)
            {
                WindowsWithinSight.Add(collidersOfComponent[collider]);
            }

            Profiler.EndSample();
        }

        private LayerMask glassLayerMask = LayerMask.GetMask("Glass");

        public override void ProcessSightSensedItems()
        {
            foreach (var item in WindowsWithinSight)
            {
                OnSensedWindowsWithinSight?.Invoke(item);
            }
            OnAfterSensedWindowsWithinSight?.Invoke();
        }
    }
}
