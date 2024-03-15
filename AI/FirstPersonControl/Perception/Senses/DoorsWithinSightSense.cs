using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using PluginAPI.Core;
using Scp914;
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
            var openedScp914ChamberDoorBelief = _fpcBotPlayer.MindRunner.GetBelief<Scp914ChamberDoor>(b => b.State == DoorState.Opened);

            var closedIntakeChamberDoor = _fpcBotPlayer.MindRunner.GetBelief<IntakeChamberDoor>(b => b.State == DoorState.Closed);
            var openedIntakeChamberDoor = _fpcBotPlayer.MindRunner.GetBelief<IntakeChamberDoor>(b => b.State == DoorState.Opened);
            var closedOutakeChamberDoor = _fpcBotPlayer.MindRunner.GetBelief<OutakeChamberDoor>(b => b.State == DoorState.Closed);
            var openedOutakeChamberDoor = _fpcBotPlayer.MindRunner.GetBelief<OutakeChamberDoor>(b => b.State == DoorState.Opened);
            foreach (var doorWithinSight in DoorsWithinSight)
            {
                if (doorWithinSight is PryableDoor pryable)
                {
                    if (doorWithinSight.RequiredPermissions.RequiredPermissions.HasFlag(KeycardPermissions.ContainmentLevelOne))
                    {
                        // door is considered to be access to SCP-914 containment chamber.
                        if (!doorWithinSight.IsConsideredOpen())
                        {
                            // closed
                            if (!closedScp914ChamberDoorBelief.Door)
                            {
                                UpdateDoorBelief(closedScp914ChamberDoorBelief, pryable);
                            }

                            if (openedScp914ChamberDoorBelief.Door)
                            {
                                UpdateDoorBelief(openedScp914ChamberDoorBelief, null as PryableDoor);
                            }
                        }
                        else
                        {
                            // opened
                            if (closedScp914ChamberDoorBelief.Door)
                            {
                                UpdateDoorBelief(closedScp914ChamberDoorBelief, null as PryableDoor);
                            }

                            if (!openedScp914ChamberDoorBelief.Door)
                            {
                                UpdateDoorBelief(openedScp914ChamberDoorBelief, pryable);
                            }
                        }
                    }
                }
                if (doorWithinSight is DoorVariant door)
                {
                    RoomIdUtils.TryFindRoom(RoomName.Lcz914, FacilityZone.LightContainment, RoomShape.Endroom, out var foundRoom);
                    if (foundRoom)
                    {
                        var scp914Room = foundRoom;

                        var intakeDoorPosition = scp914Room.transform.TransformPoint(new(3.90f, 1.00f, 4.86f));
                        if (Vector3.Distance(door.transform.position, intakeDoorPosition) < 1f)
                        {
                            // door nearby is to intake chamber
                            if (!door.IsConsideredOpen())
                            {
                                // closed
                                if (!closedIntakeChamberDoor.Door)
                                {
                                    UpdateDoorBelief(closedIntakeChamberDoor, door);
                                }

                                if (openedIntakeChamberDoor.Door)
                                {
                                    UpdateDoorBelief(openedIntakeChamberDoor, null as DoorVariant);
                                }
                            }
                            else
                            {
                                // opened
                                if (closedIntakeChamberDoor.Door)
                                {
                                    UpdateDoorBelief(closedIntakeChamberDoor, null as DoorVariant);
                                }

                                if (!openedIntakeChamberDoor.Door)
                                {
                                    UpdateDoorBelief(openedIntakeChamberDoor, door);
                                }
                            }
                        }

                        var outakeDoorPosition = scp914Room.transform.TransformPoint(new(3.91f, 1.00f, -5.87f));
                        if (Vector3.Distance(door.transform.position, outakeDoorPosition) < 1f)
                        {
                            // door nearby is to outake chamber
                            if (!door.IsConsideredOpen())
                            {
                                // closed
                                if (!closedOutakeChamberDoor.Door)
                                {
                                    UpdateDoorBelief(closedOutakeChamberDoor, door);
                                }

                                if (openedOutakeChamberDoor.Door)
                                {
                                    UpdateDoorBelief(openedOutakeChamberDoor, null as DoorVariant);
                                }
                            }
                            else
                            {
                                // opened
                                if (closedOutakeChamberDoor.Door)
                                {
                                    UpdateDoorBelief(closedOutakeChamberDoor, null as DoorVariant);
                                }

                                if (!openedOutakeChamberDoor.Door)
                                {
                                    UpdateDoorBelief(openedOutakeChamberDoor, door);
                                }
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
