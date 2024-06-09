using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Door
{
    internal record struct Segment(Vector3 Start, Vector3 End)
    {
        public Ray Ray = new(Start, End - Start);
        public float Length = Vector3.Distance(Start, End);
    }

    internal class DoorObstacle : IBelief
    {
        private readonly FpcBotNavigator navigator;

        public DoorObstacle(DoorsWithinSightSense doorsWithinSightSense, FpcBotNavigator navigator)
        {
            this.navigator = navigator;
            doorsWithinSightSense.OnSensedDoorWithinSight += OnSensedDoorWithinSight;
        }

        private readonly Queue<Vector3> removeQueue = new();

        private void OnSensedDoorWithinSight(DoorVariant door)
        {
            var doorColliders = door.GetComponentsInChildren<Collider>();

            // Remove doors not obstructing paths anymore

            var goalPositions = Doors.Where(p => p.Value == door).Select(p => p.Key);
            foreach (var goalPos in goalPositions)
            {
                var segment = Segments[goalPos];

                if (!doorColliders.Any(collider => collider.Raycast(segment.Ray, out _, segment.Length)))
                {
                    removeQueue.Enqueue(goalPos);
                }
            }

            while (removeQueue.Count > 0)
            {
                Remove(removeQueue.Dequeue());
            }

            // Add door if obstructs current navigation path

            var pathOfPoints = navigator.PointsPath;
            var segments = navigator.PathSegments;

            var hitSegment = segments
                .Select(s => new Segment(s.point, s.nextPoint))
                .Where(s => doorColliders
                    .Any(collider => collider.Raycast(s.Ray, out _, s.Length)))
                .Select(s => new Segment?(s))
                .FirstOrDefault();

            if (hitSegment.HasValue)
            {
                var newGoalPos = pathOfPoints.Last();
                Add(door, newGoalPos, hitSegment.Value);
            }
        }

        private void Add(DoorVariant door, Vector3 goalPos, Segment segment)
        {
            if (!Doors.ContainsKey(goalPos) || Doors[goalPos] != door)
            {
                Doors[goalPos] = door;
                Segments[goalPos] = segment;
                OnUpdate?.Invoke();
            }
        }

        private void Remove(Vector3 goalPos)
        {
            if (Doors.ContainsKey(goalPos))
            {
                Doors.Remove(goalPos);
                Segments.Remove(goalPos);
                OnUpdate?.Invoke();
            }
        }

        public bool IsAny => Doors.Count > 0;
        public Dictionary<Vector3, DoorVariant> Doors { get; } = new();
        public Dictionary<Vector3, Segment> Segments { get; } = new();

        public event Action OnUpdate;

        public bool Is(Vector3 goalPos)
        {
            return Doors.ContainsKey(goalPos);
        }

        public DoorVariant GetLastUninteractableDoor()
        {
            return Doors.Values.LastOrDefault(IsUniteractable);
        }

        public DoorVariant GetLastDoor(KeycardPermissions keycardPermissions)
        {
            return Doors.Values.LastOrDefault(d => IsInteractable(d) && ToKeycardPermissions(d.RequiredPermissions) == keycardPermissions);
        }

        public Vector3 GetLastGoalPosition(DoorVariant door)
        {
            return this.Doors.Last(p => p.Value == door).Key;
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
            return d is DummyDoor or ElevatorDoor;
        }

        public override string ToString()
        {
            return $"{nameof(DoorObstacle)}: {string.Join(", ", this.Doors.Values.Select(d => $"Permissions = {d.RequiredPermissions.RequiredPermissions}"))}";
        }
    }
}
