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
    internal class BotPlayer
    {
        public BotPlayer(LocalConnectionToClient connectionToClient, LocalConnectionToServer connectionToServer, ReferenceHub hub)
        {
            this._connectionToClient = connectionToClient;
            this._connectionToServer = connectionToServer;
            this._playerHub = hub;
        }

        public IEnumerator<float> MoveAsync(Vector3 direction, int timeAmount)
        {
            if (this._playerHub.roleManager.CurrentRole is IFpcRole fpcRole)
            {
                return _fpcRoleController.MoveFpcAsync(fpcRole, direction, timeAmount);
            }

            throw new InvalidOperationException("Unsupported current role on bot move.");
        }

        public IEnumerator<float> TurnAsync(Vector3 degrees, Vector3 targetDegrees)
        {
            if (this._playerHub.roleManager.CurrentRole is IFpcRole fpcRole)
            {
                return _fpcRoleController.TurnFpcAsync(fpcRole, degrees, targetDegrees);
            }

            throw new InvalidOperationException("Unsupported current role on bot turn.");
        }

        public IEnumerator<float> StartFollowingAsync(ReferenceHub playerHubToFollow)
        {
            if (this._playerHub.roleManager.CurrentRole is IFpcRole fpcRole)
            {
                return _fpcRoleController.StartFollowingFpcAsync(fpcRole, playerHubToFollow);
            }

            throw new InvalidOperationException("Unsupported current role on bot turn.");
        }

        private LocalConnectionToClient _connectionToClient;
        private LocalConnectionToServer _connectionToServer;
        private ReferenceHub _playerHub;

        private FpcBotController _fpcRoleController = new FpcBotController();
    }
}
