using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using InventorySystem.Items.Keycards;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Door
{
    internal class DoorObstacle : IBelief
    {
        private readonly FpcBotNavigator navigator;

        public DoorObstacle(DoorsWithinSightSense doorsWithinSightSense, FpcBotNavigator navigator)
        {
            doorsWithinSightSense.OnSensedDoorWithinSight += OnSensedDoorWithinSight;
            doorsWithinSightSense.OnAfterSensedDoorsWithinSight += OnAfterSensedDoorsWithinSight;
            this.navigator = navigator;
        }

        private void OnSensedDoorWithinSight(DoorVariant door)
        {
            var pathOfPoints = navigator.PointsPath;

            var rays = pathOfPoints.Zip(pathOfPoints.Skip(1), (point, nextPoint) => new Ray(point, nextPoint - point));

            var doorColliders = door.GetComponentsInChildren<Collider>();
            var doorIsObstacle = rays.Any(ray => doorColliders.Any(collider => collider.Raycast(ray, out _, 1f)));
            if (doorIsObstacle)
            {
                var goalPos = pathOfPoints.Last();
                Add(door, goalPos);
            }


        }

        private void OnAfterSensedDoorsWithinSight()
        {

        }

        private void Add(DoorVariant door, Vector3 goalPos)
        {
            if (!Doors.ContainsKey(goalPos))
            {
                Doors.Add(goalPos, door);
                OnUpdate?.Invoke();
            }
        }

        private void Remove(DoorVariant door)
        {
            throw new NotImplementedException();
        }

        public bool IsAny => Doors.Count > 0;
        public Dictionary<Vector3, DoorVariant> Doors { get; } = new();

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

        private static KeycardPermissions ToKeycardPermissions(DoorPermissions doorPermissions)
        {
            return doorPermissions.RequiredPermissions ^ KeycardPermissions.ScpOverride;
        }

        private static bool IsInteractable(DoorVariant d)
        {
            return !IsUniteractable(d);
        }

        private static bool IsUniteractable(DoorVariant d)
        {
            return d is DummyDoor or ElevatorDoor;
        }
    }
}
