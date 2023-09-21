using CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.Commands.Navigation
{
	[CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class NavCommand : ParentCommand
    {
        public override string Command { get; } = "nav";

        public override string[] Aliases { get; } = new string [] { };

        public override string Description { get; } = "Manipulates navigation graph.";

        public override void LoadGeneratedCommands()
        {
            this.RegisterCommand(new NavEditCommand());
            this.RegisterCommand(new NavLoadCommand());
            this.RegisterCommand(new NavSaveCommand());
            this.RegisterCommand(new NavNodeCommand());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = $"Please specify a valid subcommand. ({string.Join("/", Commands.Keys.ToArray())})";
            return false;
        }
    }
}
