using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using PluginAPI.Core;
using Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using System.Collections.Generic;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class DoorsWithinSightSense : SightSense, ISense
    {
        public HashSet<DoorVariant> DoorsWithinSight { get; } = new();

        public DoorsWithinSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        {
            _fpcBotPlayer = botPlayer;
        }

        public void Reset()
        {
            DoorsWithinSight.Clear();
        }

        public void ProcessSensibility(Collider collider)
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

        public void ProcessSensedItems()
        {
            foreach (var doorWithinSight in DoorsWithinSight)
            {
                if (doorWithinSight is PryableDoor pryable)
                {

                }
                if (doorWithinSight is DoorVariant door)
                {

                }
            }
        }

        private readonly FpcBotPlayer _fpcBotPlayer;
    }
}
