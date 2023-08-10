using CommandSystem;
using Mirror;
using System;

namespace TestPlugin.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class TestCommand : ICommand
    {
        public string Command => "plugin_test";

        public string[] Aliases => new string[] { };

        public string Description => "Test command";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Success response";
            return true;
        }
    }
}
