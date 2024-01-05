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
    internal class NavVertexSelectStartCommand : ICommand
    {
        public string Command { get; } = "select_start";

        public string[] Aliases { get; } = new string[] { "nvss" };

        public string Description { get; } = "Starts navigation mesh vertex auto-selection.";

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

            NavigationMeshEditor.Instance.ToggleAutoSelectingVertices(true);

            response = $"Vertex auto-selection started.";
            return true;
        }
    }
}
