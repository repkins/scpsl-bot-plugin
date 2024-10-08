﻿using CommandSystem;
using PluginAPI.Core;
using RemoteAdmin;
using SCPSLBot.Navigation.Mesh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.Commands.Navigation
{
    [CommandHandler(typeof(Nav))]
    internal class NavEditCommand : ICommand
    {
        public string Command { get; } = "edit";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Toggles editing of nav mesh.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is not PlayerCommandSender playerCommandSender)
            {
                response = "You must be in-game to use this command!";
                return false;
            }

            var navMeshEditor = NavigationMeshEditor.Instance;

            navMeshEditor.IsEditing = !navMeshEditor.IsEditing;
            navMeshEditor.PlayerEditing = navMeshEditor.IsEditing ? Player.Get(playerCommandSender) : null;

            response = $"Nav mesh editing is now {(navMeshEditor.IsEditing ? "enabled" : "disabled")}.";
            return true;
        }
    }
}
