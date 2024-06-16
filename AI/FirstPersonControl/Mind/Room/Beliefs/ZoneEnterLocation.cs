using MapGeneration;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs
{
    internal class ZoneEnterLocation : IBelief
    {
        public FacilityZone Zone { get; }
        private ZoneEnterLocation(FacilityZone zone)
        {
            Zone = zone;
        }

        private readonly RoomSightSense roomSightSense;
        public ZoneEnterLocation(FacilityZone zone, RoomSightSense roomSightSense) : this(zone)
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
        public event Action OnUpdate;

        protected void Update(Vector3? position)
        {
            if (position != Position)
            {
                Position = position;
                OnUpdate?.Invoke();
            }
        }

        public override string ToString()
        {
            return $"{nameof(ZoneEnterLocation)}({Zone}): {Position}";
        }
    }
}
