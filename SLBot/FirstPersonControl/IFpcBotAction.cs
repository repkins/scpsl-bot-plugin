using PlayerRoles.FirstPersonControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPlugin.SLBot.FirstPersonControl
{
    internal interface IFpcBotAction
    {
        void UpdatePlayer(IFpcRole fpcRole);
    }
}
