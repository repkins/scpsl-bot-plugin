using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using SCPSLBot.AI.FirstPersonControl;
using System;
using System.Collections.Generic;
using TestPlugin.LocalNetworking;
using UnityEngine;

namespace SCPSLBot.AI
{
    internal class BotHub
    {
        public FpcBotPlayer FpcBotPlayer { get; }
        public ReferenceHub PlayerHub { get; }

        public LocalConnectionToClient ConnectionToClient;
        public LocalConnectionToServer ConnectionToServer;

        public BotHub(LocalConnectionToClient connectionToClient, LocalConnectionToServer connectionToServer, ReferenceHub hub)
        {
            PlayerHub = hub;

            ConnectionToClient = connectionToClient;
            ConnectionToServer = connectionToServer;
            FpcBotPlayer = new FpcBotPlayer(this);
        }

        public IEnumerator<float> MoveAsync(Vector3 direction, int timeAmount)
        {
            if (PlayerHub.roleManager.CurrentRole is IFpcRole fpcRole)
            {
                return FpcBotPlayer.MoveFpcAsync(fpcRole, direction, timeAmount);
            }

            throw new InvalidOperationException("Unsupported current role on bot move.");
        }

        public IEnumerator<float> TurnAsync(Vector3 degrees, Vector3 targetDegrees)
        {
            if (PlayerHub.roleManager.CurrentRole is IFpcRole fpcRole)
            {
                return FpcBotPlayer.TurnFpcAsync(fpcRole, degrees, targetDegrees);
            }

            throw new InvalidOperationException("Unsupported current role on bot turn.");
        }

        public IEnumerator<float> ApproachAsync()
        {
            if (PlayerHub.roleManager.CurrentRole is IFpcRole fpcRole)
            {
                return FpcBotPlayer.FindAndApproachFpcAsync(fpcRole);
            }

            throw new InvalidOperationException("Unsupported current role on bot turn.");
        }

        public void Update()
        {
            if (PlayerHub.roleManager.CurrentRole is IFpcRole fpcRole)
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
