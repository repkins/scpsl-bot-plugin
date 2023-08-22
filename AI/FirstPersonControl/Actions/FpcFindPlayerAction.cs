using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Actions
{
    internal class FpcFindPlayerAction : IFpcAction
    {
        public ReferenceHub FoundPlayer { get; set; }

        public FpcFindPlayerAction(FpcBotPlayer fpcBotPlayer)
        {
            _fpcBotPlayer = fpcBotPlayer;
        }

        public void Reset()
        {
            FoundPlayer = null;
        }

        public void UpdatePlayer()
        {
            if (_fpcBotPlayer.Perception.FriendiesWithinSight.FirstOrDefault() is ReferenceHub otherPlayer)
            {
                var selfHub = _fpcBotPlayer.FpcRole.FpcModule.GetComponentInParent<ReferenceHub>();
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
