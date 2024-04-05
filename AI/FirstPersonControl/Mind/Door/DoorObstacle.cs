using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using InventorySystem.Items.Keycards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Door
{
    internal class DoorObstacle : IBelief
    {
        public bool IsAny { get; internal set; }
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
