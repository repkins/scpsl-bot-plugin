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

    internal class DoorObstacle : Belief<bool>
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
            DoorEntry? obstuctingEntry = null;

            foreach (var (point, nextPoint) in navigator.PathSegments)
            {
                if (!sightSense.IsPositionWithinFov(nextPoint))
                {
                    continue;
                }

                if (sightSense.IsPositionObstructed(point))
                {
                    continue;
                }

                if (!Physics.Linecast(point, nextPoint, out var hit, LayerMask.GetMask("Door")))
                {
                    continue;
                }

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
                InvokeOnUpdate();
            }
        }

        private void RemoveIfAdded(Vector3 goalPos)
        {
            if (Doors.ContainsKey(goalPos))
            {
                GoalPositions.Remove(goalPos);
                Doors.Remove(goalPos);
                InvokeOnUpdate();
            }
        }

        public bool IsAny => Doors.Count > 0;
        public Dictionary<Vector3, DoorEntry> Doors { get; } = new();
        public readonly List<Vector3> GoalPositions = new();

        public bool Is(Vector3 goalPos)
        {
            return Doors.ContainsKey(goalPos);
        }

        public TDoor GetLastDoor<TDoor>() where TDoor : DoorVariant
        {
            return GetLastDoor((d, _) => d is TDoor, out _) as TDoor;
        }

        public TDoor GetLastDoor<TDoor>(out Vector3 goalPos) where TDoor : DoorVariant
        {
            return GetLastDoor((d, _) => d is TDoor, out goalPos) as TDoor;
        }

        public DoorVariant GetLastDoor(Predicate<DoorVariant> predicate)
        {
            return GetLastDoor((b, _) => predicate(b), out _);
        }

        public DoorVariant GetLastDoor(Func<DoorVariant, KeycardPermissions, bool> predicate, out Vector3 goalPos)
        {
            if (GoalPositions.Count == 0)
            {
                goalPos = default;
                return null;
            }

            goalPos = GoalPositions.Last();

            var (lastDoor, permissions) = Doors[GoalPositions.Last()];
            return lastDoor && predicate(lastDoor, permissions) ? lastDoor: null;
        }

        public DoorVariant GetLastDoor(KeycardPermissions keycardPermissions)
        {
            return GetLastDoor(keycardPermissions, out _);
        }

        public DoorVariant GetLastDoor(KeycardPermissions keycardPermissions, out Vector3 goalPos)
        {
            return GetLastDoor((d, p) => IsInteractable(d) && ToKeycardPermissions(p) == keycardPermissions, out goalPos);
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
            return $"{nameof(DoorObstacle)}: {string.Join(", ", GoalPositions.Select(p => $"{this.Doors[p].Door.GetType().Name}: Permissions = {this.Doors[p].Permissions}"))}";
        }
    }
}
