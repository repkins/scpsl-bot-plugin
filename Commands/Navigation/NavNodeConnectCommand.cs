using CommandSystem;
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
    internal class NavNodeConnectCommand : ICommand
    {
        public string Command { get; } = "nav_node_connect";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Connects nearest node to target node.";

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
            var roomKindNode = nearestNode.RoomKindNode;

            RoomKindNode targetRoomKindNode;

            var room = nearestNode.Room.Identifier;

            if (arguments.Count > 1)
            {
                if (!int.TryParse(arguments.At(0), out var nodeId))
                {
                    response = $"Target node id should be valid integer.";
                    return false;
                }

                if (!NavigationGraph.Instance.NodesByRoomKind[roomKindNode.RoomKind].TryGet(nodeId, out targetRoomKindNode))
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
                targetRoomKindNode = NavigationGraphEditor.Instance.FindClosestNodeFacingAt(roomKindNode.RoomKind, localPosition, localForward);

                if (targetRoomKindNode == null)
                {
                    response = $"No target node at player direction.";
                    return false;
                }
            }

            if (roomKindNode.ConnectedNodes.Contains(targetRoomKindNode))
            {
                response = $"Node #{roomKindNode.Id} is already connected with node {targetRoomKindNode.Id}.";
                return false;
            }

            roomKindNode.ConnectedNodes.Add(targetRoomKindNode);

            if (!targetRoomKindNode.ConnectedNodes.Contains(roomKindNode))
            {
                targetRoomKindNode.ConnectedNodes.Add(roomKindNode);
            }

            response = $"Node #{roomKindNode.Id} connected with node #{targetRoomKindNode.Id} at {roomKindNode.RoomKind}.";

            return true;
        }
    }
}
