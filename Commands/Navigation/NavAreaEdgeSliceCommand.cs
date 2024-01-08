using CommandSystem;
using PlayerRoles;
using RemoteAdmin;
using SCPSLBot.Navigation.Mesh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.Commands.Navigation
{
    [CommandHandler(typeof(NavArea))]
    internal class NavAreaEdgeSliceCommand : ICommand
    {
        public string Command { get; } = "edge_slice";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Slices closest area edge at direction from current position.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is not PlayerCommandSender playerCommandSender)
            {
                response = "You must be in-game to use this command!";
                return false;
            }

            if (!playerCommandSender.ReferenceHub.IsAlive())
            {
                response = "Command disabled when you are not alive!";
                return false;
            }

            if (!NavigationMeshEditor.Instance.SliceClosestAreaEdge(playerCommandSender.ReferenceHub.transform.position, playerCommandSender.ReferenceHub.transform.forward))
            {
                response = $"No nearby area edge.";
                return false;
            }

            response = $"Vertex on area edge at direction created and added to area.";
            return true;
        }
    }
}
