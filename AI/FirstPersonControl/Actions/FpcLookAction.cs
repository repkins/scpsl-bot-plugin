using PlayerRoles.FirstPersonControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Actions
{
    internal class FpcLookAction : IFpcAction
    {
        public Vector3 TargetLookDirection;

        public FpcLookAction(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
        }

        public void Reset()
        { }

        public void UpdatePlayer()
        {
            var angleDiff = Vector3.SignedAngle(TargetLookDirection, _botPlayer.FpcRole.FpcModule.transform.forward, Vector3.down);
            _botPlayer.DesiredLook = new Vector3(0, angleDiff);
        }

        private FpcBotPlayer _botPlayer;
    }
}
