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
        public FacilityRoom RoomPlayerAt;

        public SpatialSense(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
        }

        public IEnumerator<JobHandle> ProcessSensibility(IEnumerable<Collider> colliders)
        { yield break; }

        public void Reset()
        { }

        public void ProcessSensedItems()
        {
            var playerPosition = _botPlayer.FpcRole.FpcModule.transform.position;
            OnSensedPlayerPosition?.Invoke(playerPosition);
        }

        private readonly FpcBotPlayer _botPlayer;
    }
}
