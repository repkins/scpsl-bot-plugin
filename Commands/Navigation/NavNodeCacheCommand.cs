﻿using CommandSystem;
using PlayerRoles;
using RemoteAdmin;
using SCPSLBot.Navigation.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.Commands.Navigation
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class NavNodeCacheCommand : ICommand
    {
        public string Command { get; } = "nav_node_cache";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Caches navigation graph node at current position.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!(sender is PlayerCommandSender playerCommandSender))
            {
                response = "You must be in-game to use this command!";
                return false;
            }

            if (!playerCommandSender.ReferenceHub.IsAlive())
            {
                response = "Command disabled when you are not alive!";
                return false;
            }

            if (!NavigationGraphEditor.Instance.CacheNode(playerCommandSender.ReferenceHub.transform.position))
            {
                response = $"Failed to cache node.";
                return false;
            }

            response = $"Node cached.";
            return true;
        }
    }
}
