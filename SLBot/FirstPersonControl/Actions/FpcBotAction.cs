using PlayerRoles.FirstPersonControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPlugin.SLBot.FirstPersonControl.Actions
{
    internal abstract class FpcBotAction : IFpcBotAction
    {
        public FpcBotAction(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
        }

        public abstract void UpdatePlayer(IFpcRole fpcRole);

        protected FpcBotPlayer _botPlayer;
    }
}
