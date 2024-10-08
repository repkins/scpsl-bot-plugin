﻿using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl;
using SCPSLBot.LocalNetworking;
using System;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace SCPSLBot.AI
{
    internal class BotHub
    {
        public readonly FpcBotPlayer FpcPlayer;

        public IBotPlayer CurrentBotPlayer { get; private set; }
        public ReferenceHub PlayerHub { get; }

        public LocalConnectionToClient ConnectionToClient;
        public LocalConnectionToServer ConnectionToServer;

        public BotHub(LocalConnectionToClient connectionToClient, LocalConnectionToServer connectionToServer, ReferenceHub hub)
        {
            PlayerHub = hub;

            ConnectionToClient = connectionToClient;
            ConnectionToServer = connectionToServer;

            FpcPlayer = new FpcBotPlayer(this);
        }

        public IEnumerator<JobHandle> Update()
        {
            Profiler.BeginSample($"{nameof(BotHub)}.{nameof(Update)}");

            var botPlayerUpdate = CurrentBotPlayer?.Update();
            if (botPlayerUpdate != null)
            {
                while (botPlayerUpdate.MoveNext())
                {
                    yield return botPlayerUpdate.Current;
                }
            }

            Profiler.EndSample();
        }

        public void OnRoleChanged(PlayerRoleBase prevRole, PlayerRoleBase newRole)
        {
            if (newRole is FpcStandardRoleBase fpcRole)
            {
                FpcPlayer.FpcRole = fpcRole;
                CurrentBotPlayer = FpcPlayer;
            }
            else
            {
                CurrentBotPlayer = null;
            }

            if (CurrentBotPlayer != null)
            {
                CurrentBotPlayer.OnRoleChanged();
            }

            Log.Info($"Bot got new role assigned. Role Id: {newRole.RoleTypeId}");
            Log.Debug($"Type of role: {newRole.GetType()}");
        }

        public override string ToString()
        {
            return $"{nameof(BotHub)}: {PlayerHub}";
        }
    }
}
