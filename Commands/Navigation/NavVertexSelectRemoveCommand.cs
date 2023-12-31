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
    internal class NavVertexSelectRemoveCommand : ICommand
    {
        public string Command { get; } = "select_remove";

        public string[] Aliases { get; } = new string[] { "nvsr" };

        public string Description { get; } = "Removes navigation mesh vertex from selection nearby at current position.";

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

            if (!NavigationMeshEditor.Instance.RemoveVertexFromSelection(playerCommandSender.ReferenceHub.transform.position))
            {
                response = $"No vertex nearby to perform this command.";
                return false;
            }

            response = $"Vertex removed from selection.";
            return true;
        }
    }
}
