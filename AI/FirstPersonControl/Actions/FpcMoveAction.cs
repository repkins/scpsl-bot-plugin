using PlayerRoles.FirstPersonControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Actions
{
    internal class FpcMoveAction : IFpcAction
    {
        public Vector3 TargetPosition;

        public FpcMoveAction(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
        }

        public void Reset()
        { }

        public void UpdatePlayer()
        {
            _botPlayer.DesiredMoveDirection = (TargetPosition - _botPlayer.FpcRole.FpcModule.transform.position).normalized;
        }

        private FpcBotPlayer _botPlayer;
    }
}
