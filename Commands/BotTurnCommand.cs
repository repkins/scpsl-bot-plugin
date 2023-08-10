using CommandSystem;
using System;
using TestPlugin.SLBot;
using UnityEngine;

namespace TestPlugin.Commands
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class BotTurnCommand : ICommand
    {
        public string Command => "bot_turn";

        public string[] Aliases => new string[] { };

        public string Description => "Turns bot";

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

            if (!Enum.TryParse(arguments.At(1), out BotTurnDirection direction))
            {
                response = $"Turn direction should be either of: \n" +
                    $"{nameof(BotTurnDirection.Up)}, \n" +
                    $"{nameof(BotTurnDirection.Down)}, \n" +
                    $"{nameof(BotTurnDirection.Right)}, \n" +
                    $"{nameof(BotTurnDirection.Left)}. \n";
                return false;
            }

            if (!int.TryParse(arguments.At(2), out var targetAngle))
            {
                response = string.Concat(new string[]
                {
                    "Target angle should be valid integer."
                });
                return false;
            }

            var turnDirectionDegrees = Vector3.zero;
            var targetDegrees = Vector3.zero;
            switch (direction)
            {
                case BotTurnDirection.Up:
                    turnDirectionDegrees.x = 60f;
                    targetDegrees.x = targetAngle;
                    break;
                case BotTurnDirection.Down:
                    turnDirectionDegrees.x = -60f;
                    targetDegrees.x = targetAngle;
                    break;
                case BotTurnDirection.Right:
                    turnDirectionDegrees.y = 60f;
                    targetDegrees.y = targetAngle;
                    break;
                case BotTurnDirection.Left:
                    turnDirectionDegrees.y = -60f;
                    targetDegrees.y = targetAngle;
                    break;
            }

            BotManager.Instance.DebugBotTurn(playerId, turnDirectionDegrees, targetDegrees);

            response = "Success";
            return true;
        }
    }
}
