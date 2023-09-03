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
    internal class NavNodeInfoCommand : ICommand
    {
        public string Command { get; } = "nav_node_info";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Prints nearest node info at current position.";

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

            var position = playerCommandSender.ReferenceHub.transform.position;
            var node = NavigationGraph.Instance.FindNearestNode(position);
            if (node == null)
            {
                response = $"No nearby node found at position {position}.";
                return false;
            }

            var (roomName, roomShape, roomZone) = node.RoomKindNode.RoomKind;
            var room = RoomIdentifier.AllRoomIdentifiers.First(r => (r.Name, r.Shape, (RoomZone)r.Zone) == (roomName, roomShape, roomZone));
            var nodePosition = room.transform.TransformPoint(node.LocalPosition);
            var distance = Vector3.Distance(nodePosition, position);

            response = $"Node found at {node.LocalPosition} in {node.RoomKindNode.RoomKind} from distance {distance}.";
            return true;
        }
    }
}
