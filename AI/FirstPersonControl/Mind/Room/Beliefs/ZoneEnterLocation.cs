using MapGeneration;
using SCPSLBot.AI.FirstPersonControl.Mind.Spacial;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs
{
    internal class ZoneEnterLocation : Location
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
            if (foreignRoomAreaOfTargetZone != null && foreignRoomAreaOfTargetZone.Room.Identifier.Zone != this.roomSightSense.RoomWithin.Zone)
            {
                var enterPosition = foreignRoomAreaOfTargetZone.CenterPosition;
                AddPosition(enterPosition);
            }
        }

        public Vector3? Position => this.Positions.Any() ? this.Positions.First() : null;

        public override string ToString()
        {
            return $"{nameof(ZoneEnterLocation)}({Zone}): {Position}";
        }
    }
}
