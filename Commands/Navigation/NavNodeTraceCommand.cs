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
    [CommandHandler(typeof(NavNode))]
    internal class NavNodeTraceCommand : ICommand
    {
        public string Command { get; } = "trace";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Traces nodes path from cached node to node at current position.";

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

            if (!NavigationGraphEditor.Instance.TracePath(playerCommandSender.ReferenceHub.transform.position))
            {
                response = $"Failed to trace nodes path.";
                return false;
            }

            response = $"Path traced.";
            return true;
        }
    }
}
