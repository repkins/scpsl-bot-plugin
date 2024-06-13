using CommandSystem;
using PlayerRoles;
using RemoteAdmin;
using SCPSLBot.Navigation.Mesh;
using System;

namespace SCPSLBot.Commands.Navigation
{
    [CommandHandler(typeof(NavArea))]
    internal class NavAreaDisconnectCommand : ICommand
    {
        public string Command { get; } = "disconnect";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Deletes connection from cached area to area within.";

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

            if (!NavigationMeshEditor.Instance.DeleteConnection(playerCommandSender.ReferenceHub.transform.position))
            {
                response = "Failed to delete connection!";
                return false;
            }

            response = $"Connection from cached area to area within is deleted.";
            return true;
        }
    }
}
