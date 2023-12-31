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
    internal class NavVertexDeleteCommand : ICommand
    {
        public string Command { get; } = "delete";

        public string[] Aliases { get; } = new string[] { "nvd" };

        public string Description { get; } = "Deletes navigation mesh vertex nearby at current position.";

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

            if (!NavigationMeshEditor.Instance.DeleteVertex(playerCommandSender.ReferenceHub.transform.position))
            {
                response = $"No vertex to be removed.";
                return false;
            }

            response = $"Vertex removed.";
            return true;
        }
    }
}
