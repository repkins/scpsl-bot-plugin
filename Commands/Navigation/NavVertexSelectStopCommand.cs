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
    internal class NavVertexSelectStopCommand : ICommand
    {
        public string Command { get; } = "select_stop";

        public string[] Aliases { get; } = new string[] { "nvsst" };

        public string Description { get; } = "Stops navigation mesh vertex auto-selection.";

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

            NavigationMeshEditor.Instance.ToggleAutoSelectingVertices(false);

            response = $"Vertex auto-selection stopped.";
            return true;
        }
    }
}
