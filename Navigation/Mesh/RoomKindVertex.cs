using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.Navigation.Mesh
{
    internal class RoomKindVertex
    {
        public Vector3 LocalPosition { get; set; }

        public RoomKindVertex(Vector3 position)
        {
            LocalPosition = position;
        }
    }
}
