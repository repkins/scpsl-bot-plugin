using System;
using System.Collections.Generic;
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

        public override void ProcessSensibility(Collider collider)
        {
            if (collider.GetComponentInParent<BreakableWindow>() is BreakableWindow window
                && !WindowsWithinSight.Contains(window))
            {
                if (IsWithinSight(collider, window))
                {
                    WindowsWithinSight.Add(window);
                }
            }
        }

        public override void ProcessSensedItems()
        {
            base.ProcessSensedItems();

            foreach (var item in WindowsWithinSight)
            {
                OnSensedWindowsWithinSight?.Invoke(item);
            }
            OnAfterSensedWindowsWithinSight?.Invoke();
        }
    }
}
