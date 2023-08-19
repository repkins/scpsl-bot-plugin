using MEC;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TestPlugin.SLBot.FirstPersonControl.Actions
{
    internal class FpcBotFindPlayerAction : IFpcBotAction
    {
        public ReferenceHub FoundPlayer { get; set; }

        public void OnEnter()
        {
            FoundPlayer = null;
        }

        public void UpdatePlayer(IFpcRole fpcRole)
        {
            if (Physics.Raycast(fpcRole.FpcModule.transform.position, fpcRole.FpcModule.transform.forward, out var hit))
            {
                var selfHub = fpcRole.FpcModule.GetComponentInParent<ReferenceHub>();
                if (hit.collider.GetComponentInParent<ReferenceHub>() is ReferenceHub hitHub && hitHub != selfHub)
                {
                    Log.Info($"{selfHub} found player to follow: {hitHub}.");
                    FoundPlayer = hitHub;
                }
            }
        }
    }
}
