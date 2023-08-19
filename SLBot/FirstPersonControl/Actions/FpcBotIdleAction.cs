using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPlugin.SLBot.FirstPersonControl.Actions
{
    internal class FpcBotIdleAction : IFpcBotAction
    {
        public bool IsRoleChanged { get; private set; }
        
        public FpcBotIdleAction(FpcBotPlayer fpcBotPlayer)
        {
            fpcBotPlayer.OnChangedRole += OnRoleChanged;
        }

        public void OnEnter()
        { }

        public void UpdatePlayer(IFpcRole fpcRole)
        { }

        private void OnRoleChanged(PlayerRoleBase prevRole, PlayerRoleBase newRole)
        {
            IsRoleChanged = true;
        }
    }
}
