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
            var room = RoomIdUtils.RoomAtPositionRaycasts(playerPosition);
            if (!room)
            {
                return;
            }

            if (room.Name == RoomName.Lcz914 && ChamberDoorLocalPlane.GetSide(room.transform.InverseTransformPoint(playerPosition)) == true)
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

        public override string ToString()
        {
            return $"{GetType().Name}(IsInside = {IsInside})";
        }

        private static Plane ChamberDoorLocalPlane = new(Vector3.right, new Vector3(-2.8f, 1f, 0f));
        public RoomIdentifier Scp914RoomInstance { get; }
    }
}
