using MapGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.Navigation.Graph
{
    internal class RoomKindEdge
    {
        public (RoomName, RoomShape, RoomZone) RoomKind { get; set; }

        public Vector3 LocalPosition { get; set; }
        public Vector3 LocalDirection { get; set; }
        public float Extents { get; set; }
    }
}
