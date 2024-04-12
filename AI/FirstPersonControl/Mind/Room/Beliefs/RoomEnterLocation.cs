using PluginAPI.Core;
using PluginAPI.Core.Zones;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs
{
    internal class RoomEnterLocation : IBelief
    {
        private readonly RoomSightSense roomSightSense;

        public RoomEnterLocation(RoomSightSense roomSightSense)
        {
            this.roomSightSense = roomSightSense;
            this.roomSightSense.OnAfterSensedForeignRooms += OnAfterSensedForeignRooms;
        }

        private Dictionary<FacilityRoom, float> roomsLastVisitTime = new();
        private FacilityRoom enteringAreaRoom;
        private FacilityRoom prevRoomWithin;

        private void OnAfterSensedForeignRooms()
        {
            var foreignRoomAreas = this.roomSightSense.ForeignRoomsAreas;
            var enteringArea = foreignRoomAreas
                .OrderBy(fa => roomsLastVisitTime.TryGetValue(fa.Room, out var time) ? time : 0f)
                .FirstOrDefault();

            if (enteringArea is null)
            {
                Log.Warning($"No entering foreign area found. Current room within {this.roomSightSense.RoomWithin.Name}");
                return;
            }

            var roomWithin = this.roomSightSense.RoomWithin.ApiRoom;
            enteringAreaRoom = enteringArea.Room;

            if (roomWithin != prevRoomWithin && prevRoomWithin != null)
            {
                roomsLastVisitTime[prevRoomWithin] = Time.time;
            }

            prevRoomWithin = roomWithin;

            var enterPosition = enteringArea.CenterPosition;
            Update(enterPosition);
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
            return $"{nameof(RoomEnterLocation)}({enteringAreaRoom.Identifier.Name})";
        }
    }
}
