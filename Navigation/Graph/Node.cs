using MapGeneration;
using PluginAPI.Core.Zones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.Navigation.Graph
{
    internal class Node
    {
        public Vector3 LocalPosition { get; set; }
        public float Radius { get; set; }
        public List<Node> ConnectedNodes { get; } = new List<Node>();

        public int Id { get; private set; }
        public FacilityRoom Room { get; private set; }
        public NodeTemplate Template { get; private set; }

        public Node(NodeTemplate nodeTemplate, FacilityRoom room)
        {
            Id = nodeTemplate.Id;
            LocalPosition = nodeTemplate.LocalPosition;
            Radius = nodeTemplate.Radius;
            Room = room;
            Template = nodeTemplate;
        }

        public Node(Vector3 localPosition)
        {
            LocalPosition = localPosition;
        }
    }
}
