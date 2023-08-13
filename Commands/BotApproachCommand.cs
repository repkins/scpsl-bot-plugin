using CommandSystem;
using System;
using TestPlugin.SLBot;
using UnityEngine;

namespace TestPlugin.Commands
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class BotApproachCommand : ICommand
    {
        public string Command => "bot_come";

        public string[] Aliases => new string[] { };

        public string Description => "Moves bot towards player in sight.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 1)
            {
                response = "To execute this command provide at least 1 arguments!";
                return false;
            }

            if (!int.TryParse(arguments.At(0), out var playerId))
            {
                response = $"Player number should be valid integer.";
                return false;
            }

            BotManager.Instance.DebugBotApproach(playerId);

            response = "Success";
            return true;
        }
    }
}
