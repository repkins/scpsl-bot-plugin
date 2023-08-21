using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Actions
{
    internal class FpcBotFindPlayerAction : IFpcBotAction
    {
        public ReferenceHub FoundPlayer { get; set; }

        public FpcBotFindPlayerAction(FpcBotPlayer fpcBotPlayer)
        {
            _fpcBotPlayer = fpcBotPlayer;
        }

        public void OnEnter()
        {
            FoundPlayer = null;
        }

        public void UpdatePlayer(IFpcRole fpcRole)
        {
            if (_fpcBotPlayer.Perception.FriendiesWithinSight.FirstOrDefault() is ReferenceHub otherPlayer)
            {
                var selfHub = fpcRole.FpcModule.GetComponentInParent<ReferenceHub>();
                if (otherPlayer != selfHub)
                {
                    Log.Info($"{selfHub} found player to follow: {otherPlayer}.");
                    FoundPlayer = otherPlayer;
                }
            }
        }

        private FpcBotPlayer _fpcBotPlayer;
    }
}
