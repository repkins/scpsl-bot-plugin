using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.World;
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

        public void UpdateBeliefs()
        {
            var numPryableDoors = 0u;
            var pryableWithinSightBelief = _fpcBotPlayer.MindRunner.GetBelief<DoorWithinSight<PryableDoor>>();
            foreach (var doorWithinSight in DoorsWithinSight)
            {
                if (doorWithinSight is PryableDoor gate)
                {
                    if (pryableWithinSightBelief.Door is null)
                    {
                        UpdateDoorBelief(pryableWithinSightBelief, gate);
                    }
                    numPryableDoors++;
                }
            }
            if (numPryableDoors <= 0 && pryableWithinSightBelief.Door is not null)
            {
                UpdateDoorBelief(pryableWithinSightBelief, null as PryableDoor);
            }
        }

        private static void UpdateDoorBelief<T, I>(T doorBelief, I door) where T : DoorBase<I> where I : DoorVariant
        {
            doorBelief.Update(door);
            Log.Debug($"{doorBelief.GetType().Name} updated: {door}");
        }

        private readonly FpcBotPlayer _fpcBotPlayer;
    }
}
