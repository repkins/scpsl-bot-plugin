using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        public override void ProcessSensibility(IEnumerable<Collider> colliders)
        {
            var collidersOfComponent = colliders
                .Select(c => (c, Window: c.GetComponentInParent<BreakableWindow>()))
                .Where(t => t.Window is not null && !WindowsWithinSight.Contains(t.Window));

            var withinSight = this.GetWithinSight(collidersOfComponent);

            foreach (var collider in withinSight)
            {
                WindowsWithinSight.Add(collider.GetComponentInParent<BreakableWindow>());
            }
        }

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
