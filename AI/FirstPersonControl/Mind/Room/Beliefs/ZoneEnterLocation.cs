using MapGeneration;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs
{
    internal class ZoneEnterLocation : Belief<bool>
    {
        public FacilityZone Zone { get; }
        public FacilityZone FromZone { get; }
        public ZoneEnterLocation(FacilityZone zone, FacilityZone fromZone, RoomSightSense roomSightSense) : this(roomSightSense)
        {
            Zone = zone;
            FromZone = fromZone;
        }

        private readonly RoomSightSense roomSightSense;
        private ZoneEnterLocation(RoomSightSense roomSightSense)
        {
            this.roomSightSense = roomSightSense;
            this.roomSightSense.OnAfterSensedForeignRooms += OnAfterSensedForeignRooms;
        }

        private void OnAfterSensedForeignRooms()
        {
            var foreignRoomAreaOfTargetZone = this.roomSightSense.ForeignRoomsAreas.Find(r => r.Room.Identifier.Zone == Zone);
            if (foreignRoomAreaOfTargetZone != null)
            {
                var enterPosition = foreignRoomAreaOfTargetZone.CenterPosition;
                Update(enterPosition);
            }
        }

        public Vector3? Position { get; private set; }

        protected void Update(Vector3? position)
        {
            if (position != Position)
            {
                Position = position;
                InvokeOnUpdate();
            }
        }

        public override string ToString()
        {
            return $"{nameof(ZoneEnterLocation)}({Zone}): {Position}";
        }
    }
}
