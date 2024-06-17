using Hints;
using Interactables;
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

    internal record struct DoorEntry(DoorVariant Door, KeycardPermissions Permissions);

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

            DoorEntry? obstuctingEntry = null;

            foreach (var pathPoint in pathOfPoints)
            {
                if (!sightSense.IsPositionWithinFov(pathPoint))
                {
                    continue;
                }

                if (sightSense.IsPositionObstructed(pathPoint, out var hit))
                {
                    var hitDoor = hit.collider.GetComponentInParent<DoorVariant>();
                    if (hitDoor && !hitDoor.IsConsideredOpen())
                    {
                        var interactable = hit.collider.GetComponent<InteractableCollider>();
                        var target = interactable?.Target;
                        obstuctingEntry = target is DoorVariant interactableDoorTarget
                            ? new(hitDoor, interactableDoorTarget.RequiredPermissions.RequiredPermissions)
                            : new(hitDoor, hitDoor.RequiredPermissions.RequiredPermissions);
                        break;
                    }
                }
            }

            var goalPos = navigator.GoalPosition;

            if (obstuctingEntry.HasValue)
            {
                AddOrReplace(obstuctingEntry.Value, goalPos);
            }
            else
            {
                RemoveIfAdded(goalPos);
            }
        }

        private void AddOrReplace(DoorEntry doorEntry, Vector3 goalPos)
        {
            if (!Doors.ContainsKey(goalPos) || Doors[goalPos] != doorEntry)
            {
                if (Doors.ContainsKey(goalPos))
                {
                    GoalPositions.Remove(goalPos);
                }
                GoalPositions.Add(goalPos);
                Doors[goalPos] = doorEntry;
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
        public Dictionary<Vector3, DoorEntry> Doors { get; } = new();
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

            var lastDoor = Doors[GoalPositions.Last()].Door;
            return lastDoor && IsUniteractable(lastDoor) ? lastDoor : null;
        }

        public DoorVariant GetLastDoor(KeycardPermissions keycardPermissions)
        {
            return GetLastDoor(keycardPermissions, out _);
        }

        public DoorVariant GetLastDoor(KeycardPermissions keycardPermissions, out Vector3 goalPos)
        {
            if (GoalPositions.Count == 0)
            {
                goalPos = default;
                return null;
            }

            goalPos = GoalPositions.Last();

            var (lastDoor, permissions) = Doors[goalPos];
            if (lastDoor && IsInteractable(lastDoor) && ToKeycardPermissions(permissions) == keycardPermissions)
            {
                return lastDoor;
            }
            else
            {
                return null;
            }
        }

        private static KeycardPermissions ToKeycardPermissions(KeycardPermissions doorPermissions)
        {
            return doorPermissions & ~KeycardPermissions.ScpOverride;
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
            return $"{nameof(DoorObstacle)}: {string.Join(", ", GoalPositions.Select(p => $"Permissions = {this.Doors[p].Permissions}"))}";
        }
    }
}
