using PluginAPI.Core.Zones;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.Navigation.Mesh
{
    internal class Area
    {
        public RoomKindArea RoomKindArea { get; }
        public FacilityRoom Room { get; }

        public Vector3 CenterPosition => Room.Transform.InverseTransformPoint(RoomKindArea.LocalCenterPosition);

        public IEnumerable<Area> ConnectedAreas => RoomKindArea.ConnectedRoomKindAreas.Select(k => k.AreasOfRoom[Room]).Concat(ForeignConnectedAreas);

        public List<Area> ForeignConnectedAreas { get; } = new();

        public Area(RoomKindArea roomKindArea, FacilityRoom room)
        {
            Room = room;
            RoomKindArea = roomKindArea;

            RoomKindArea.AreasOfRoom.Add(Room, this);
        }

        ~Area()
        {
            RoomKindArea.AreasOfRoom.Remove(Room);
        }
    }
}
