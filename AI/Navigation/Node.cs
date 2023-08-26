﻿using MapGeneration;
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
        public int Id { get; set; }
        public Vector3 LocalPosition { get; set; }

        public (RoomName, RoomShape) RoomNameShape { get; set; }

        public List<int> ConnectedNodeIndices { get; } = new List<int>();

        public List<Node> ConnectedNodes { get; } = new List<Node>();

        public Node(Vector3 localPosition, int[] connectedNodeIndices)
        {
            LocalPosition = localPosition;
            ConnectedNodeIndices.AddRange(connectedNodeIndices);
        }
    }
}
