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
            _fpcBotPlayer = botPlayer;
        }

        public void Reset()
        {
            WindowsWithinSight.Clear();
        }

        public void ProcessSensibility(Collider collider)
        {
            if (collider.GetComponentInParent<BreakableWindow>() is BreakableWindow door
                && !WindowsWithinSight.Contains(door))
            {
                if (IsWithinSight(collider, door))
                {
                    WindowsWithinSight.Add(door);
                }
            }
        }

        public void ProcessSensedItems()
        {
            foreach (var item in WindowsWithinSight)
            {
                OnSensedWindowsWithinSight?.Invoke(item);
            }
            OnAfterSensedWindowsWithinSight?.Invoke();
        }

        private readonly FpcBotPlayer _fpcBotPlayer;
    }
}
