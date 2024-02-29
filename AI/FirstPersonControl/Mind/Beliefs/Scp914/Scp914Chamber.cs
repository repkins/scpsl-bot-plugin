using MapGeneration;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Scp914
{
    internal class Scp914Chamber : IBelief
    {
        public bool IsInside { get; private set; }

        public event Action OnUpdate;

        public Scp914Chamber(SpatialSense spatialSense)
        {
            spatialSense.OnSensedPlayerPosition += ProcessSensedPlayerPosition;
        }

        private void ProcessSensedPlayerPosition(Vector3 playerPosition)
        {
            EnsureAssignedScp914Room();

            var room = RoomIdUtils.RoomAtPositionRaycasts(playerPosition);
            if (room == scp914RoomInstance && sideLocalPlane.GetSide(room.transform.InverseTransformPoint(playerPosition)) == true)
            {
                if (!IsInside)
                {
                    Update(isInside: true);
                }
            }
            else
            {
                if (IsInside)
                {
                    Update(isInside: false);
                }
            }
        }

        private void Update(bool isInside)
        {
            IsInside = isInside;
            OnUpdate?.Invoke();
        }

        private void EnsureAssignedScp914Room()
        {
            if (!scp914RoomInstance)
            {
                RoomIdUtils.TryFindRoom(RoomName.Lcz914, FacilityZone.LightContainment, RoomShape.Endroom, out var foundRoom);
                if (foundRoom)
                {
                    scp914RoomInstance = foundRoom;
                }
            }
        }

        public override string ToString()
        {
            return $"{GetType().Name}(IsInside = {IsInside})";
        }

        private static Plane sideLocalPlane = new(Vector3.right, new Vector3(-2.8f, 1f, 0f));
        public Vector3 InsideNormal => scp914RoomInstance.transform.TransformDirection(sideLocalPlane.normal);

        private RoomIdentifier scp914RoomInstance;
    }
}
