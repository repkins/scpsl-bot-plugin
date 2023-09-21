using CommandSystem;
using PlayerRoles;
using PluginAPI.Core;
using RemoteAdmin;
using SCPSLBot.Navigation.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.Commands.Navigation
{
    [CommandHandler(typeof(NavCommand))]
    internal class NavEditCommand : ICommand
    {
        public string Command { get; } = "edit";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Toggles editing of nav graph.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!(sender is PlayerCommandSender playerCommandSender))
            {
                response = "You must be in-game to use this command!";
                return false;
            }

            var navGraphEditor = NavigationGraphEditor.Instance;

            navGraphEditor.IsEditing = !navGraphEditor.IsEditing;
            navGraphEditor.PlayerEditing = navGraphEditor.IsEditing ? Player.Get(playerCommandSender) : null;

            response = $"Nav graph editing is now {(navGraphEditor.IsEditing ? "enabled" : "disabled")}.";
            return true;
        }
    }
}
