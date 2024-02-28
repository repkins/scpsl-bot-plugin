using System;
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

        public void ProcessSensibility(Collider collider)
        { }

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
