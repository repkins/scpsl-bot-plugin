using MapGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.Navigation.Graph
{
    internal class NodeTemplate
    {
        public int Id { get; set; }
        public Vector3 LocalPosition { get; set; }
        public float Radius { get; set; }

        public (RoomName, RoomShape) RoomNameShape { get; set; }

        public List<NodeTemplate> ConnectedNodes { get; } = new List<NodeTemplate>();

        public NodeTemplate(NodeTemplate nodeTemplate)
        {
            Id = nodeTemplate.Id;
            LocalPosition = nodeTemplate.LocalPosition;
            Radius = nodeTemplate.Radius;
            RoomNameShape = nodeTemplate.RoomNameShape;
        }

        public NodeTemplate(Vector3 localPosition)
        {
            LocalPosition = localPosition;
        }
    }
}
