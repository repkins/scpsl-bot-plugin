using CommandSystem;
using Mirror;
using PluginAPI.Core;
using System;
using TestPlugin.SLBot;

namespace TestPlugin.Commands
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    internal class BotAddCommand : ICommand
    {
        public string Command => "bot_add";

        public string[] Aliases => new string[] { };

        public string Description => "Bot add";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            BotManager.Instance.AddBotPlayer();

            response = "Done.";
            return true;
        }

        private void LogInMessage(NetworkDiagnostics.MessageInfo obj)
        {
            Log.Info(obj.ToString());
        }
    }
}
