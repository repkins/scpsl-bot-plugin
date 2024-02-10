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
        public Vector3 TargetLookLocalDirection { private get; set; }

        public FpcLookAction(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
        }

        public void Reset()
        { }

        public void UpdatePlayer()
        {
            var angleDiff = Vector3.SignedAngle(TargetLookLocalDirection, _botPlayer.FpcRole.FpcModule.transform.forward, Vector3.down);
        }

        private FpcBotPlayer _botPlayer;
    }
}
