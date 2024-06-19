using PluginAPI.Core.Zones;
using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class SpatialSense : ISense
    {
        public event Action<Vector3> OnSensedPlayerPosition;

        public SpatialSense(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
        }

        public void ProcessSensedItems()
        {
            var playerPosition = _botPlayer.PlayerPosition;
            OnSensedPlayerPosition?.Invoke(playerPosition);
        }

        public void ProcessEnter(Collider other)
        {
        }

        public void ProcessExit(Collider other)
        {
        }

        public IEnumerator<JobHandle> ProcessSensibility()
        {
            yield break;
        }

        private readonly FpcBotPlayer _botPlayer;
    }
}
