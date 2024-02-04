using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Movement
{
    internal class FpcMove
    {
        //public Vector3 DesiredDirection { get; set; } = Vector3.zero;
        public Vector3 DesiredLocalDirection { get; set; } = Vector3.zero;

        private readonly FpcBotPlayer botPlayer;

        public FpcMove(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public IEnumerator<float> ToFpcAsync(Vector3 localDirection, int timeAmount)
        {
            DesiredLocalDirection = localDirection;

            yield return Timing.WaitForSeconds(timeAmount);

            DesiredLocalDirection = Vector3.zero;

            yield break;
        }
    }
}
