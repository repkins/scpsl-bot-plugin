using MapGeneration;
using PluginAPI.Core;
using PluginAPI.Core.Zones;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs
{
    internal class RoomEnterLocation : IBelief
    {
        private readonly RoomSightSense roomSightSense;

        public RoomEnterLocation(RoomSightSense roomSightSense)
        {
            this.roomSightSense = roomSightSense;
            this.roomSightSense.OnAfterSensedForeignRooms += OnAfterSensedForeignRooms;

            //seed = (int)DateTime.Now.Ticks;
            //Log.Debug($"seed for room selection: {seed}");
        }

        private FacilityRoom enteringAreaRoom;

        private readonly Dictionary<FacilityRoom, float> roomsLastVisitTime = new();
        private FacilityRoom prevRoomWithin;

        //private readonly int seed;

        private void OnAfterSensedForeignRooms()
        {
            var roomWithin = this.roomSightSense.RoomWithin.ApiRoom;

            // Room change check
            if (roomWithin != prevRoomWithin && prevRoomWithin != null)
            {
                roomsLastVisitTime[prevRoomWithin] = Time.time;
            }
            prevRoomWithin = roomWithin;

            // Reached target room check
            if (enteringAreaRoom == roomWithin || enteringAreaRoom == null)
            {
                //var prevRandomState = Random.state;
                //Random.InitState(seed);

                var foreignRoomAreas = this.roomSightSense.ForeignRoomsAreas;
                var enteringArea = foreignRoomAreas
                    .Where(fa => fa.Room.Identifier.Zone == roomWithin.Identifier.Zone)
                    .Where(fa => fa.Room.Identifier.Shape != RoomShape.Endroom || checkpointRoomNames.Contains(fa.Room.Identifier.Name))
                    .OrderBy(fa => roomsLastVisitTime.TryGetValue(fa.Room, out var time) ? time : -Random.Range(0f, 4f))
                    .FirstOrDefault();

                if (enteringArea is null)
                {
                    Log.Warning($"No entering foreign area found. Current room within {this.roomSightSense.RoomWithin.Name}");
                    return;
                }

                enteringAreaRoom = enteringArea.Room;

                var enterPosition = enteringArea.CenterPosition;
                Update(enterPosition);

                //Random.state = prevRandomState;
            }
        }

        private readonly HashSet<RoomName> checkpointRoomNames = new()
        {
            RoomName.LczCheckpointA,
            RoomName.LczCheckpointB,
            RoomName.HczCheckpointA,
            RoomName.HczCheckpointB,
        };

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
            return $"{nameof(RoomEnterLocation)}: {enteringAreaRoom?.Identifier.Name}, {enteringAreaRoom?.Identifier.Shape}, {enteringAreaRoom?.Identifier.Zone}";
        }
    }
}
