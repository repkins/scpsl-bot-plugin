using MapGeneration;
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
            spatialSense.OnSensedLocalPlayerPosition += ProcessSensedPlayerPosition;
        }

        private void ProcessSensedPlayerPosition(Vector3 playerPosition)
        {
            var room = RoomIdUtils.RoomAtPositionRaycasts(playerPosition);
            if (room)
            {
                var localPlayerPosition = room.transform.InverseTransformPoint(playerPosition);

                if (chamberDoorLocalPlane.GetSide(localPlayerPosition) == true)
                {
                    Update(isInside: true);
                }
                else
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

        private Plane chamberDoorLocalPlane = new(Vector3.forward, new Vector3(0f, 0f, 0f));
    }
}
