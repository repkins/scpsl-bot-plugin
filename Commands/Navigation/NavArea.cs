using CommandSystem;
using System;
using System.Linq;

namespace SCPSLBot.Commands.Navigation
{
    [CommandHandler(typeof(Nav))]
    internal class NavArea : ParentCommand
    {
        public override string Command { get; } = "area";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Manipulates navigation mesh areas.";

        public override void LoadGeneratedCommands()
        {
            this.RegisterCommand(new NavAreaMakeCommand());
            this.RegisterCommand(new NavAreaDissolveCommand());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            var subCommands = Commands.Keys.ToArray();
            response = $"Please specify a valid subcommand. ({string.Join("/", subCommands)})";
            return false;
        }
    }
}
