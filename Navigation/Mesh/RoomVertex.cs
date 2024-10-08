﻿using PluginAPI.Core;
using PluginAPI.Core.Zones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.Navigation.Mesh
{
    internal class RoomVertex
    {
        public RoomKindVertex RoomKindVertex { get; }
        public FacilityRoom Room { get; }

        public Vector3 Position => Room.Transform.TransformPoint(RoomKindVertex.LocalPosition);

        public Vector3 LocalPosition => RoomKindVertex.LocalPosition;

        public RoomVertex(RoomKindVertex roomKindVertex, FacilityRoom room)
        {
            RoomKindVertex = roomKindVertex;
            Room = room;
        }

        public override string ToString()
        {
            var idx = NavigationMesh.Instance.VerticesByRoom[Room].IndexOf(this);
            return $"#{idx} {RoomKindVertex.RoomKind}";
        }
    }
}
