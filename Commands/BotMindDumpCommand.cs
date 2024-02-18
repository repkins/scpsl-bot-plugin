using CommandSystem;
using SCPSLBot.AI;
using System;
using UnityEngine;

namespace TestPlugin.Commands
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class BotMindDumpCommand : ICommand
    {
        public string Command => "bot_mind_dump";

        public string[] Aliases => new string[] { };

        public string Description => "Prints mind graph of specified bot player.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 1)
            {
                response = "To execute this command provide at least 3 arguments!";
                return false;
            }

            if (!int.TryParse(arguments.At(0), out var playerId))
            {
                response = $"Player number should be valid integer.";
                return false;
            }

            BotManager.Instance.DebugBotMindDump(playerId);

            response = "Success";
            return true;
        }
    }
}
