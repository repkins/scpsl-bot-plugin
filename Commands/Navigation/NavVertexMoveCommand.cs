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
    internal class NavVertexMoveCommand : ICommand
    {
        public string Command { get; } = "move";

        public string[] Aliases { get; } = new string[] { "nvm" };

        public string Description { get; } = "Moves nearby or selected navigation mesh vertex to current position.";

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

            if (!NavigationMeshEditor.Instance.MoveVertex(playerCommandSender.ReferenceHub.transform.position))
            {
                response = $"No vertex nearby or no selected vertex to perform operation on.";
                return false;
            }

            response = $"Vertex moved.";
            return true;
        }
    }
}
