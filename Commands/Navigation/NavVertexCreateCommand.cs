using CommandSystem;
using PlayerRoles;
using RemoteAdmin;
using SCPSLBot.Navigation.Mesh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.Commands.Navigation
{
    [CommandHandler(typeof(NavVertex))]
    internal class NavVertexCreateCommand : ICommand
    {
        public string Command { get; } = "create";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Creates navigation mesh vertex at current position.";

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

            var vertex = NavigationMeshEditor.Instance.CreateVertex(playerCommandSender.ReferenceHub.transform.position);

            response = $"Vertex at local position {vertex.LocalPosition} added.";
            return true;
        }
    }
}
