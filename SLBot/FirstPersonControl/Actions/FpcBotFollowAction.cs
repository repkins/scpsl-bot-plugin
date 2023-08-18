using PlayerRoles.FirstPersonControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TestPlugin.SLBot.FirstPersonControl.Actions
{
    internal class FpcBotFollowAction : IFpcBotAction
    {
        public ReferenceHub TargetToFollow { get; set; }

        public FpcBotFollowAction(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
        }

        public void OnEnter()
        {
            TargetToFollow = null;
        }

        public void UpdatePlayer(IFpcRole fpcRole)
        {
            if (!TargetToFollow)
            {
                return;
            }

            var directionToTarget = (TargetToFollow.transform.position - fpcRole.FpcModule.transform.position).normalized;

            if (Physics.Raycast(fpcRole.FpcModule.transform.position, directionToTarget, out var hit))
            {
                if (hit.transform == TargetToFollow.transform)
                {
                    TargetLastKnownLocation = hit.transform.position;
                }
            }

            if (Vector3.Distance(TargetToFollow.transform.position, fpcRole.FpcModule.transform.position) >= 1f)
            {
                _botPlayer.DesiredMoveDirection = (TargetLastKnownLocation - fpcRole.FpcModule.transform.position).normalized;

                var angleDiff = Vector3.SignedAngle(_botPlayer.DesiredMoveDirection, fpcRole.FpcModule.transform.forward, Vector3.down);
                _botPlayer.DesiredLook = new Vector3(0, angleDiff);
            }
            else
            {
                _botPlayer.DesiredMoveDirection = Vector3.zero;
                _botPlayer.DesiredLook = Vector3.zero;
            }
        }

        private FpcBotPlayer _botPlayer;
        private Vector3 TargetLastKnownLocation { get; set; }
    }
}
