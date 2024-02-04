using CommandSystem;
using MapGeneration;
using Mirror;
using PlayerRoles;
using PluginAPI.Core;
using RemoteAdmin;
using System;

namespace TestPlugin.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class PositionLocalCommand : ICommand
    {
        public string Command => "position_local";

        public string[] Aliases => new string[] { };

        public string Description => "Returns calling player relative position to room player currenty in.";

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

            var playerPos = playerCommandSender.ReferenceHub.transform.position;
            var room = RoomIdUtils.RoomAtPositionRaycasts(playerPos);

            var relPlayerPos = room.transform.InverseTransformPoint(playerPos);
            var roomKind = (room.Name, room.Shape, room.Zone);

            response = $"Relative player {playerCommandSender.Nickname} position to room of kind {roomKind}: {relPlayerPos}";
            Log.Info(response);

            return true;
        }
    }
}
