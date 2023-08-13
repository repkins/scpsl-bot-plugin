using PlayerRoles.FirstPersonControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TestPlugin.SLBot.FirstPersonControl.Actions
{
    internal class FpcBotFollowAction : FpcBotAction
    {
        public FpcBotFollowAction(FpcBotPlayer botPlayer) : base(botPlayer)
        {
        }

        public ReferenceHub TargetToFollow { get; set; }

        public override void UpdatePlayer(IFpcRole fpcRole)
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
    }
}
