using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Door.Scp914;
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
            var numClosedScp914RoomDoors = 0u;

            var closedScp914RoomDoorWithinSightBelief = _fpcBotPlayer.MindRunner.GetBelief<ClosedScp914ChamberDoorWithinSight>(b => true);
            foreach (var doorWithinSight in DoorsWithinSight)
            {
                if (doorWithinSight is PryableDoor pryable)
                {
                    // Checking if door is SCP-914 access door by it's unique permissions.
                    if (doorWithinSight.RequiredPermissions.RequiredPermissions.HasFlag(KeycardPermissions.ContainmentLevelOne))
                    {
                        if (!doorWithinSight.IsConsideredOpen())
                        {
                            if (!closedScp914RoomDoorWithinSightBelief.Door)
                            {
                                UpdateDoorBelief(closedScp914RoomDoorWithinSightBelief, pryable);
                            }
                            numClosedScp914RoomDoors++;
                        }
                    }
                }
            }

            if (numClosedScp914RoomDoors <= 0 && closedScp914RoomDoorWithinSightBelief.Door)
            {
                UpdateDoorBelief(closedScp914RoomDoorWithinSightBelief, null as PryableDoor);
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
