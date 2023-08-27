using CommandSystem;
using PlayerRoles;
using PluginAPI.Core;
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
    internal class NavEditCommand : ICommand
    {
        public string Command { get; } = "nav_edit";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Toggles editing of nav graph.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!(sender is PlayerCommandSender playerCommandSender))
            {
                response = "You must be in-game to use this command!";
                return false;
            }

            NavigationGraph.Instance.IsEditing = !NavigationGraph.Instance.IsEditing;
            NavigationGraph.Instance.PlayerEditing = NavigationGraph.Instance.IsEditing ? Player.Get(playerCommandSender) : null;

            response = $"Nav graph editing is now {(NavigationGraph.Instance.IsEditing ? "enabled" : "disabled")}.";
            return true;
        }
    }
}
