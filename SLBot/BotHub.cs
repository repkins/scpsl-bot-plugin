using MEC;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using TestPlugin.LocalNetworking;
using TestPlugin.SLBot.FirstPersonControl;
using UnityEngine;

namespace TestPlugin.SLBot
{
    internal class BotHub
    {
        public FpcBotPlayer FpcBotPlayer { get; }
        public ReferenceHub PlayerHub { get; } 

        public LocalConnectionToClient ConnectionToClient;
        public LocalConnectionToServer ConnectionToServer;

        public BotHub(LocalConnectionToClient connectionToClient, LocalConnectionToServer connectionToServer, ReferenceHub hub)
        {
            this.PlayerHub = hub;

            this.ConnectionToClient = connectionToClient;
            this.ConnectionToServer = connectionToServer;
            FpcBotPlayer = new FpcBotPlayer(this);
        }

        public IEnumerator<float> MoveAsync(Vector3 direction, int timeAmount)
        {
            if (this.PlayerHub.roleManager.CurrentRole is IFpcRole fpcRole)
            {
                return FpcBotPlayer.MoveFpcAsync(fpcRole, direction, timeAmount);
            }

            throw new InvalidOperationException("Unsupported current role on bot move.");
        }

        public IEnumerator<float> TurnAsync(Vector3 degrees, Vector3 targetDegrees)
        {
            if (this.PlayerHub.roleManager.CurrentRole is IFpcRole fpcRole)
            {
                return FpcBotPlayer.TurnFpcAsync(fpcRole, degrees, targetDegrees);
            }

            throw new InvalidOperationException("Unsupported current role on bot turn.");
        }

        public IEnumerator<float> ApproachAsync()
        {
            if (this.PlayerHub.roleManager.CurrentRole is IFpcRole fpcRole)
            {
                return FpcBotPlayer.FindAndApproachFpcAsync(fpcRole);
            }

            throw new InvalidOperationException("Unsupported current role on bot turn.");
        }

        public void Update()
        {
            if (this.PlayerHub.roleManager.CurrentRole is IFpcRole fpcRole)
            {
                FpcBotPlayer.Update(fpcRole);
            }
        }

        public void OnRoleChanged(PlayerRoleBase prevRole, PlayerRoleBase newRole)
        {
            FpcBotPlayer.OnRoleChanged(prevRole, newRole);
        }
    }
}
