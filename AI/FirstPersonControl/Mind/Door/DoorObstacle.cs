using Hints;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Door
{
    internal record struct Segment(Vector3 Start, Vector3 End)
    {
        public readonly Ray Ray = new(Start, End - Start);
        public readonly float Length = Vector3.Distance(Start, End);
    }

    internal class DoorObstacle : IBelief
    {
        private readonly FpcBotNavigator navigator;

        private readonly SightSense sightSense;

        public DoorObstacle(SightSense sightSense, FpcBotNavigator navigator)
        {
            this.navigator = navigator;
            this.sightSense = sightSense;

            sightSense.OnAfterSightSensing += OnAfterSightSensing;
        }

        private void OnAfterSightSensing()
        {
            var pathOfPoints = navigator.PointsPath;
            if (pathOfPoints.Count == 0)
            {
                return;
            }

            var goalPos = pathOfPoints.Last();

            DoorVariant obstructingDoor = null;

            foreach (var pathPoint in pathOfPoints)
            {
                if (!sightSense.IsPositionWithinFov(pathPoint))
                {
                    continue;
                }

                if (sightSense.IsPositionObstructed(pathPoint, out var hit))
                {
                    var door = hit.collider.GetComponentInParent<DoorVariant>();
                    if (door && !door.IsConsideredOpen())
                    {
                        obstructingDoor = door;
                        break;
                    }
                }
            }

            if (obstructingDoor)
            {
                AddOrReplace(obstructingDoor, goalPos);
            }
            else
            {
                RemoveIfAdded(goalPos);
            }
        }

        private void AddOrReplace(DoorVariant door, Vector3 goalPos)
        {
            if (!Doors.ContainsKey(goalPos) || Doors[goalPos] != door)
            {
                if (Doors.ContainsKey(goalPos))
                {
                    GoalPositions.Remove(goalPos);
                }
                GoalPositions.Add(goalPos);
                Doors[goalPos] = door;
                OnUpdate?.Invoke();
            }
        }

        private void RemoveIfAdded(Vector3 goalPos)
        {
            if (Doors.ContainsKey(goalPos))
            {
                GoalPositions.Remove(goalPos);
                Doors.Remove(goalPos);
                OnUpdate?.Invoke();
            }
        }

        public bool IsAny => Doors.Count > 0;
        public Dictionary<Vector3, DoorVariant> Doors { get; } = new();
        private readonly List<Vector3> GoalPositions = new();

        public event Action OnUpdate;

        public bool Is(Vector3 goalPos)
        {
            return Doors.ContainsKey(goalPos);
        }

        public DoorVariant GetLastUninteractableDoor()
        {
            if (GoalPositions.Count == 0)
            {
                return null;
            }

            var lastDoor = Doors[GoalPositions.Last()];
            return lastDoor && IsUniteractable(lastDoor) ? lastDoor : null;
        }

        public DoorVariant GetLastDoor(KeycardPermissions keycardPermissions)
        {
            return GetLastDoor(keycardPermissions, out _, true);
        }

        public DoorVariant GetLastDoor(KeycardPermissions keycardPermissions, out Vector3 goalPos, bool debug = false)
        {
            if (GoalPositions.Count == 0)
            {
                goalPos = default;
                return null;
            }

            goalPos = GoalPositions.Last();

            var lastDoor = Doors[goalPos];

            if (debug)
            {
                Log.Debug($"{ToKeycardPermissions(lastDoor.RequiredPermissions)} == {keycardPermissions}");
            }

            if (lastDoor && IsInteractable(lastDoor) && ToKeycardPermissions(lastDoor.RequiredPermissions) == keycardPermissions)
            {
                return lastDoor;
            }
            else
            {
                return null;
            }
        }

        private static KeycardPermissions ToKeycardPermissions(DoorPermissions doorPermissions)
        {
            return doorPermissions.RequiredPermissions & ~KeycardPermissions.ScpOverride;
        }

        private static bool IsInteractable(DoorVariant d)
        {
            return !IsUniteractable(d);
        }

        private static bool IsUniteractable(DoorVariant d)
        {
            return d is DummyDoor or ElevatorDoor or BasicNonInteractableDoor;
        }

        public override string ToString()
        {
            return $"{nameof(DoorObstacle)}: {string.Join(", ", GoalPositions.Select(p => $"Permissions = {this.Doors[p].RequiredPermissions.RequiredPermissions}"))}";
        }
    }
}
