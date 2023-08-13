using MEC;
using PlayerRoles.FirstPersonControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TestPlugin.SLBot.FirstPersonControl.Actions
{
    internal class FpcBotFindPlayerAction : FpcBotAction
    {
        public ReferenceHub FoundPlayer { get; set; }

        public FpcBotFindPlayerAction(FpcBotPlayer botPlayer) : base(botPlayer)
        {
        }

        public override void UpdatePlayer(IFpcRole fpcRole)
        {
            if (Physics.Raycast(fpcRole.FpcModule.transform.position, fpcRole.FpcModule.transform.forward, out var hit))
            {
                if (hit.collider.GetComponentInParent<ReferenceHub>() is ReferenceHub hitHub)
                {
                    FoundPlayer = hitHub;
                }
            }
        }

        public IEnumerator<float> WaitForFoundPlayer()
        {
            yield return Timing.WaitUntilTrue(() => FoundPlayer != null);
        }
    }
}
