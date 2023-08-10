using CommandSystem;
using System;
using TestPlugin.SLBot;
using UnityEngine;

namespace TestPlugin.Commands
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class BotMoveCommand : ICommand
    {
        public string Command => "bot_move";

        public string[] Aliases => new string[] { };

        public string Description => "Moves bot";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 3)
            {
                response = "To execute this command provide at least 3 arguments!";
                return false;
            }

            if (!int.TryParse(arguments.At(0), out var playerId))
            {
                response = $"Player number should be valid integer.";
                return false;
            }

            if (!Enum.TryParse(arguments.At(1), out BotMoveDirection direction))
            {
                response = $"Direction should be either of: \n" +
                    $"{nameof(BotMoveDirection.Forward)}, \n" +
                    $"{nameof(BotMoveDirection.Backward)}, \n" +
                    $"{nameof(BotMoveDirection.Right)}, \n" +
                    $"{nameof(BotMoveDirection.Left)}. \n";
                return false;
            }

            if (!int.TryParse(arguments.At(2), out var timeAmount))
            {
                response = string.Concat(new string[]
                {
                    "Time amount should be valid integer."
                });
                return false;
            }

            var directionVector = Vector3.zero;
            switch (direction)
            {
                case BotMoveDirection.Forward:
                    directionVector = Vector3.forward;
                    break;
                case BotMoveDirection.Backward:
                    directionVector = Vector3.back;
                    break;
                case BotMoveDirection.Right:
                    directionVector = Vector3.right;
                    break;
                case BotMoveDirection.Left:
                    directionVector = Vector3.left;
                    break;
            }

            BotManager.Instance.DebugBotMove(playerId, directionVector, timeAmount);

            response = "Success";
            return true;
        }
    }
}
