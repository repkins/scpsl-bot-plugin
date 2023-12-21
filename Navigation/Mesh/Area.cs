using PluginAPI.Core.Zones;
using System.Collections.Generic;
using System.Linq;

namespace SCPSLBot.Navigation.Mesh
{
    internal class Area
    {
        public RoomKindArea RoomKindArea { get; private set; }
        public FacilityRoom Room { get; private set; }

        public IEnumerable<Area> ConnectedAreas => RoomKindArea.ConnectedAreas.Select(k => k.AreasOfRoom[Room]).Concat(ForeignAreas);

        public List<Area> ForeignAreas { get; } = new();

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
