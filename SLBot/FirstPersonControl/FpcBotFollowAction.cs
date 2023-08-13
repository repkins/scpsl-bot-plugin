using PlayerRoles.FirstPersonControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TestPlugin.SLBot.FirstPersonControl
{
    internal class FpcBotFollowAction : IFpcBotAction
    {
        public ReferenceHub TargetToFollow { get; set; }
        
        public FpcBotFollowAction(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
        }

        public void UpdatePlayer(IFpcRole fpcRole)
        {
            if (TargetToFollow)
            {
                var directionToTarget = (TargetToFollow.transform.position - fpcRole.FpcModule.transform.position).normalized;

                _botPlayer.DesiredMoveDirection = directionToTarget;

                var angleDiff = Vector3.SignedAngle(directionToTarget, fpcRole.FpcModule.transform.forward, Vector3.down);
                _botPlayer.DesiredLook = new Vector3(0, angleDiff);
            }
            else
            {
                _botPlayer.DesiredMoveDirection = Vector3.zero;
                _botPlayer.DesiredLook = Vector3.zero;
            }
        }

        private FpcBotPlayer _botPlayer;
    }
}
