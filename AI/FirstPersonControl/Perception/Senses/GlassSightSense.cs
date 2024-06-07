using Interactables.Interobjects.DoorUtils;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class GlassSightSense : SightSense<BreakableWindow>, ISense
    {
        public HashSet<BreakableWindow> WindowsWithinSight => ComponentsWithinSight;

        public event Action<BreakableWindow> OnSensedWindowsWithinSight;
        public event Action OnAfterSensedWindowsWithinSight;

        public GlassSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        {
        }

        private LayerMask glassLayerMask = LayerMask.GetMask("Glass");
        protected override LayerMask LayerMask => glassLayerMask;

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
