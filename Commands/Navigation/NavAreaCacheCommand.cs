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
    [CommandHandler(typeof(NavArea))]
    internal class NavAreaCacheCommand : ICommand
    {
        public string Command { get; } = "cache";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Caches navigation graph area at current position.";

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

            if (!NavigationMeshEditor.Instance.CacheArea(playerCommandSender.ReferenceHub.transform.position))
            {
                response = $"Failed to cache area.";
                return false;
            }

            response = $"Area cached.";
            return true;
        }
    }
}
