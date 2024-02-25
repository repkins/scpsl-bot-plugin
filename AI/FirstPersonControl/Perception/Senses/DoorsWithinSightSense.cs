using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Scp914;
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
            var closedScp914ChamberDoorBelief = _fpcBotPlayer.MindRunner.GetBelief<Scp914ChamberDoor>(b => b.State == DoorState.Closed);
            foreach (var doorWithinSight in DoorsWithinSight)
            {
                if (doorWithinSight is PryableDoor pryable)
                {
                    // Checking if door is SCP-914 access door by it's unique permissions.
                    if (doorWithinSight.RequiredPermissions.RequiredPermissions.HasFlag(KeycardPermissions.ContainmentLevelOne))
                    {
                        if (!doorWithinSight.IsConsideredOpen())
                        {
                            if (!closedScp914ChamberDoorBelief.Door)
                            {
                                UpdateDoorBelief(closedScp914ChamberDoorBelief, pryable);
                            }
                        }
                        else
                        {
                            if (closedScp914ChamberDoorBelief.Door)
                            {
                                UpdateDoorBelief(closedScp914ChamberDoorBelief, null as PryableDoor);
                            }
                        }
                    }
                }
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
