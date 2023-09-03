using CommandSystem;
using PlayerRoles;
using RemoteAdmin;
using SCPSLBot.Navigation.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.Commands.Navigation
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class NavNodeRemoveCommand : ICommand
    {
        public string Command { get; } = "nav_node_remove";

        public string[] Aliases { get; } = new string[] { "nnr" };

        public string Description { get; } = "Removes near navigation graph node to current position.";

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

            if (!NavigationGraphEditor.Instance.RemoveNode(playerCommandSender.ReferenceHub.transform.position))
            {
                response = $"No node to be removed.";
                return false;
            }

            response = $"Node removed.";
            return true;
        }
    }
}
