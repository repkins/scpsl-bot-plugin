using CommandSystem;
using PlayerRoles;
using PluginAPI.Core;
using RemoteAdmin;
using SCPSLBot.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.Commands.Navigation
{
    [CommandHandler(typeof(Nav))]
    internal class NavSaveCommand : ICommand
    {
        public string Command { get; } = "save";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Saves navigation mesh to storage.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is not PlayerCommandSender playerCommandSender)
            {
                response = "You must be in-game to use this command!";
                return false;
            }

            NavigationSystem.Instance.SaveMesh();

            response = $"Navigation mesh saved.";
            return true;
        }
    }
}
