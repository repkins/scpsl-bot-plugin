﻿using CommandSystem;
using MapGeneration;
using PlayerRoles;
using RemoteAdmin;
using SCPSLBot.Navigation.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.Commands.Navigation
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class NavNodeDisconnectCommand : ICommand
    {
        public string Command { get; } = "nav_node_disconnect";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Disconnects nearest node to target node.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!(sender is PlayerCommandSender playerCommandSender))
            {
                response = "You must be in-game to use this command!";
                return false;
            }

            if (!playerCommandSender.ReferenceHub.IsAlive())
            {
                response = "Command disabled when you are not alive!";
                return false;
            }

            var playerPosition = playerCommandSender.ReferenceHub.transform.position;
            var nearestNode = NavigationGraph.Instance.FindNearestNode(playerPosition);
            if (nearestNode == null)
            {
                response = $"No nearby node found at position {playerPosition}.";
                return false;
            }
            var nodeTemplate = nearestNode.Template;

            NodeTemplate targetNodeTemplate = null;

            var (roomName, roomShape) = nodeTemplate.RoomNameShape;
            var room = RoomIdentifier.AllRoomIdentifiers.First(r => r.Name == roomName && r.Shape == roomShape);

            if (arguments.Count > 1)
            {
                if (!int.TryParse(arguments.At(0), out var nodeId))
                {
                    response = $"Target node id should be valid integer.";
                    return false;
                }

                if (!NavigationGraph.Instance.NodeTemplatesByRoom[nodeTemplate.RoomNameShape].TryGet(nodeId, out targetNodeTemplate))
                {
                    response = $"No target node exists at index {nodeId}.";
                    return false;
                }
            }
            else
            {
                var playerCameraTransform = playerCommandSender.ReferenceHub.PlayerCameraReference;
                var cameraPosition = playerCameraTransform.position;
                var cameraForward = playerCameraTransform.forward;

                var localPosition = room.transform.InverseTransformPoint(cameraPosition);
                var localForward = room.transform.InverseTransformDirection(cameraForward);
                targetNodeTemplate = NavigationGraphEditor.Instance.FindClosestNodeFacingAt(nodeTemplate.RoomNameShape, localPosition, localForward);

                if (targetNodeTemplate == null)
                {
                    response = $"No target node at player direction.";
                    return false;
                }
            }

            if (!nodeTemplate.ConnectedNodes.Contains(targetNodeTemplate))
            {
                response = $"Node #{nodeTemplate.Id} is not connected with node {targetNodeTemplate.Id}.";
                return false;
            }

            nodeTemplate.ConnectedNodes.Remove(targetNodeTemplate);

            foreach (var roomOfNameShape in NavigationGraph.Instance.NodesByRoom.Where(r => (r.Key.Identifier.Name, r.Key.Identifier.Shape) == nodeTemplate.RoomNameShape))
            {
                var node = roomOfNameShape.Value.Find(n => n.Template == nodeTemplate);
                var targetNode = roomOfNameShape.Value.Find(n => n.Template == targetNodeTemplate);
                node.ConnectedNodes.Remove(targetNode);
            }

            response = $"Node #{nodeTemplate.Id} disconnected with node #{targetNodeTemplate.Id}.";

            return true;
        }
    }
}
