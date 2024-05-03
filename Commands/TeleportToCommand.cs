using CommandSystem;
using MapGeneration;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using RemoteAdmin;
using System;
using UnityEngine;

namespace TestPlugin.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class TeleportRoCommand : ICommand
    {
        public string Command => "teleport";

        public string[] Aliases => new string[] { };

        public string Description => "Teleports calling noclip player to specified position.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is not PlayerCommandSender playerCommandSender)
            {
                response = "You must be in-game to use this command!";
                return false;
            }

            if (arguments.Count < 3)
            {
                response = "To execute this command provide at least 3 arguments!";
                return false;
            }

            if (!float.TryParse(arguments.At(0), out var x))
            {
                response = $"X should be valid float.";
                return false;
            }
            if (!float.TryParse(arguments.At(1), out var y))
            {
                response = $"Y should be valid float.";
                return false;
            }
            if (!float.TryParse(arguments.At(2), out var z))
            {
                response = $"Z should be valid float.";
                return false;
            }

            var targetPlayerPos = new Vector3(x, y, z);
            var result = playerCommandSender.ReferenceHub.TryOverridePosition(targetPlayerPos, Vector3.zero);

            response = $"Player {playerCommandSender.Nickname} teleported to specified position if successful.";
            Log.Info(response);

            return true;
        }
    }
}
