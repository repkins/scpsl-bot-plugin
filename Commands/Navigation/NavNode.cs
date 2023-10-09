using CommandSystem;
using System;
using System.Linq;

namespace SCPSLBot.Commands.Navigation
{
    [CommandHandler(typeof(Nav))]
    internal class NavNode : ParentCommand
    {
        public override string Command { get; } = "node";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Manipulates navigation graph nodes.";

        public override void LoadGeneratedCommands()
        {
            this.RegisterCommand(new NavNodeAddCommand());
            this.RegisterCommand(new NavNodeRemoveCommand());
            this.RegisterCommand(new NavNodeConnectCommand());
            this.RegisterCommand(new NavNodeDisconnectCommand());
            this.RegisterCommand(new NavNodeCacheCommand());
            this.RegisterCommand(new NavNodeTraceCommand());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            var subCommands = Commands.Keys.ToArray();
            response = $"Please specify a valid subcommand. ({string.Join("/", subCommands)})";
            return false;
        }
    }
}