using MapGeneration;
using MEC;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using SCPSLBot.AI.NavigationSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.AI
{
    internal class NavigationManager
    {
        public static NavigationManager Instance { get; } = new NavigationManager();

        public bool IsEditing { get; set; }
        public Player PlayerEditing { get; set; }
        public Node LastEditingNode { get; set; }

        public static Dictionary<(RoomName, RoomShape), List<Node>> NodesByRoom { get; } = new Dictionary<(RoomName, RoomShape), List<Node>>()
        {
            { 
                (RoomName.LczClassDSpawn, RoomShape.Endroom), new List<Node>()
                {
                    new Node() { LocalPosition = new Vector3(-22.50f, 0.96f, 0.00f), RoomNameShape = (RoomName.LczClassDSpawn, RoomShape.Endroom) },
                    new Node() { LocalPosition = new Vector3(-17.71f, 0.96f, 0.00f), RoomNameShape = (RoomName.LczClassDSpawn, RoomShape.Endroom) },
                    new Node() { LocalPosition = new Vector3(-13.82f, 0.96f, 0.00f), RoomNameShape = (RoomName.LczClassDSpawn, RoomShape.Endroom) },
                    new Node() { LocalPosition = new Vector3(-10.02f, 0.96f, 0.00f), RoomNameShape = (RoomName.LczClassDSpawn, RoomShape.Endroom) },
                    new Node() { LocalPosition = new Vector3(-6.27f, 0.96f, 0.00f), RoomNameShape = (RoomName.LczClassDSpawn, RoomShape.Endroom) },
                    new Node() { LocalPosition = new Vector3(-2.35f, 0.96f, 0.00f), RoomNameShape = (RoomName.LczClassDSpawn, RoomShape.Endroom) },
                    new Node() { LocalPosition = new Vector3(1.45f, 0.96f, 0.00f), RoomNameShape = (RoomName.LczClassDSpawn, RoomShape.Endroom) },
                    new Node() { LocalPosition = new Vector3(5.20f, 0.96f, -0.00f), RoomNameShape = (RoomName.LczClassDSpawn, RoomShape.Endroom) },
                    new Node() { LocalPosition = new Vector3(5.45f, 0.96f, -4.07f), RoomNameShape = (RoomName.LczClassDSpawn, RoomShape.Endroom) },
                    new Node() { LocalPosition = new Vector3(5.06f, 0.96f, 4.01f), RoomNameShape = (RoomName.LczClassDSpawn, RoomShape.Endroom) },
                    new Node() { LocalPosition = new Vector3(1.62f, 0.96f, -4.37f), RoomNameShape = (RoomName.LczClassDSpawn, RoomShape.Endroom) },
                    new Node() { LocalPosition = new Vector3(1.27f, 0.96f, 4.20f), RoomNameShape = (RoomName.LczClassDSpawn, RoomShape.Endroom) },
                    new Node() { LocalPosition = new Vector3(-2.12f, 0.96f, -4.18f), RoomNameShape = (RoomName.LczClassDSpawn, RoomShape.Endroom) },
                    new Node() { LocalPosition = new Vector3(-2.62f, 0.96f, 4.35f), RoomNameShape = (RoomName.LczClassDSpawn, RoomShape.Endroom) },
                    new Node() { LocalPosition = new Vector3(-5.87f, 0.96f, -4.43f), RoomNameShape = (RoomName.LczClassDSpawn, RoomShape.Endroom) },
                    new Node() { LocalPosition = new Vector3(-6.42f, 0.96f, 4.37f), RoomNameShape = (RoomName.LczClassDSpawn, RoomShape.Endroom) },
                    new Node() { LocalPosition = new Vector3(-9.73f, 0.96f, -4.29f), RoomNameShape = (RoomName.LczClassDSpawn, RoomShape.Endroom) },
                    new Node() { LocalPosition = new Vector3(-10.21f, 0.96f, 4.38f), RoomNameShape = (RoomName.LczClassDSpawn, RoomShape.Endroom) },
                    new Node() { LocalPosition = new Vector3(-13.46f, 0.96f, -4.30f), RoomNameShape = (RoomName.LczClassDSpawn, RoomShape.Endroom) },
                    new Node() { LocalPosition = new Vector3(-14.06f, 0.96f, 4.29f), RoomNameShape = (RoomName.LczClassDSpawn, RoomShape.Endroom) },
                    new Node() { LocalPosition = new Vector3(-17.39f, 0.96f, -4.58f), RoomNameShape = (RoomName.LczClassDSpawn, RoomShape.Endroom) },
                    new Node() { LocalPosition = new Vector3(-17.88f, 0.96f, 4.45f), RoomNameShape = (RoomName.LczClassDSpawn, RoomShape.Endroom) },
                }
            }
        };

        public void Init()
        {
            EventManager.RegisterEvents(this);

            Timing.RunCoroutine(RunEditingVisuals());
        }

        [PluginEvent(PluginAPI.Enums.ServerEventType.MapGenerated)]
        public void OnMapGenerated()
        {
            // Connect door waypoints
        }

        public IEnumerator<float> RunEditingVisuals()
        {
            while (true)
            {
                UpdateEditingVisuals();

                yield return Timing.WaitForOneFrame;
            }
        }

        public void UpdateEditingVisuals()
        {
            if (PlayerEditing != null)
            {
                var player = PlayerEditing;

                var nearestNode = FindNearestNode(player.Position);
                
                if (nearestNode != LastEditingNode)
                {
                    LastEditingNode = nearestNode;

                    if (nearestNode == null)
                    {
                        player.ClearBroadcasts();
                    }
                    else
                    {
                        var message = $"Node in {nearestNode.RoomNameShape} at local {nearestNode.LocalPosition}";
                        player.SendBroadcast(message, 60, shouldClearPrevious: true);
                    }
                }
            }
        }

        public Node FindNearestNode(Vector3 position, float radius = 1f)
        {
            var room = RoomIdUtils.RoomAtPosition(position);
            
            if (!NodesByRoom.TryGetValue((room.Name, room.Shape), out var roomNodes))
            {
                Log.Warning($"Can't find nodes at room {(room.Name, room.Shape)}.");
                return null;
            }

            var radiusSqr = Mathf.Pow(radius, 2);
            var localPosition = room.transform.InverseTransformPoint(position);

            var nodesWithinRadius = roomNodes.Select(node => (node, distSqr: Vector3.SqrMagnitude(node.LocalPosition - localPosition)))
                .Where(t => t.distSqr < radiusSqr);

            if (!nodesWithinRadius.Any())
            {
                return null;
            }

            return nodesWithinRadius
                .Aggregate((a, c) => c.distSqr < a.distSqr ? c : a)
                .node;
        }

        public Node AddNode(Vector3 position)
        {
            var room = RoomIdUtils.RoomAtPosition(position);

            if (!NodesByRoom.TryGetValue((room.Name, room.Shape), out var roomNodes))
            {
                roomNodes = new List<Node>();
                NodesByRoom.Add((room.Name, room.Shape), roomNodes);
            }

            var newNode = new Node() {
                LocalPosition = room.transform.InverseTransformPoint(position),
                RoomNameShape = (room.Name, room.Shape),
            };

            roomNodes.Add(newNode);

            Log.Info($"Node at local position {newNode.LocalPosition} added under room {(room.Name, room.Shape)}.");

            return newNode;
        }

        private NavigationManager()
        { }
    }
}
