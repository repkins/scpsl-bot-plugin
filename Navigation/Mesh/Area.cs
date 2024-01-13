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

        public Vector3 CenterPosition => Room.Transform.TransformPoint(RoomKindArea.LocalCenterPosition);
        public Vector3 LocalCenterPosition => RoomKindArea.LocalCenterPosition;

        public IEnumerable<Area> ConnectedAreas => RoomKindArea.ConnectedRoomKindAreas.Select(k => k.AreasOfRoom[Room]).Concat(ForeignConnectedAreas);
        public Dictionary<Area, (RoomVertex From, RoomVertex To)> ConnectedAreaEdges { get; } = new();

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

        public override string ToString()
        {
            return $"#{NavigationMesh.Instance.AreasByRoom[Room].IndexOf(this)} {RoomKindArea.RoomKind}";
        }
    }
}
