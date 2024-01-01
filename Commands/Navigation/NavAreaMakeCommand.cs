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
    internal class NavAreaMakeCommand : ICommand
    {
        public string Command { get; } = "make";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Makes new navigation mesh area from selection.";

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

            var area = NavigationMeshEditor.Instance.MakeArea(playerCommandSender.ReferenceHub.transform.position);

            response = $"Area at local center position {area.LocalCenterPosition} created.";
            return true;
        }
    }
}
