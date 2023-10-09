using CommandSystem;
using PlayerRoles;
using PluginAPI.Core;
using RemoteAdmin;
using SCPSLBot.Navigation;
using SCPSLBot.Navigation.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.Commands.Navigation
{
    [CommandHandler(typeof(Nav))]
    internal class NavLoadCommand : ICommand
    {
        public string Command { get; } = "load";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Re-loads navigation graph from storage.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!(sender is PlayerCommandSender playerCommandSender))
            {
                response = "You must be in-game to use this command!";
                return false;
            }

            NavigationSystem.Instance.ResetNodes();
            NavigationSystem.Instance.LoadNodes();
            NavigationSystem.Instance.InitRoomNodes();  // Assuming map is already generated.

            response = $"Navigation graph re-loaded.";
            return true;
        }
    }
}
