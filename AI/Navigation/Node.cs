using MapGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.AI.NavigationSystem
{
    internal class Node
    {
        public Vector3 LocalPosition { get; set; }

        public (RoomName, RoomShape) RoomNameShape { get; set; }
    }
}
