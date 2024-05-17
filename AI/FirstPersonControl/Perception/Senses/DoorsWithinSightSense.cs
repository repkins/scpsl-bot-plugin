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

        public override void ProcessSensibility(Collider collider)
        {
            if (collider.GetComponentInParent<DoorVariant>() is DoorVariant door
                && !DoorsWithinSight.Contains(door))
            {
                if (IsWithinSight(collider, door))
                {
                    DoorsWithinSight.Add(door);
                }
            }
        }

        public override void ProcessSensedItems()
        {
            base.ProcessSensedItems();

            OnBeforeSensedDoorsWithinSight?.Invoke();
            foreach (var doorWithinSight in DoorsWithinSight)
            {
                OnSensedDoorWithinSight?.Invoke(doorWithinSight);
            }
            OnAfterSensedDoorsWithinSight?.Invoke();
        }
    }
}
