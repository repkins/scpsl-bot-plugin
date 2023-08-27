using CommandSystem;
using PlayerRoles;
using RemoteAdmin;
using SCPSLBot.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class NavNodeAddCommand : ICommand
    {
        public string Command { get; } = "nav_node_add";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Adds navigation graph node to current position.";

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

            var node = NavigationGraph.Instance.AddNode(playerCommandSender.ReferenceHub.transform.position);

            response = $"Node at local position {node.LocalPosition} added.";
            return true;
        }
    }
}
