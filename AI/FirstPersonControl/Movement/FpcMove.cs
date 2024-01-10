using MEC;
using SCPSLBot.Navigation.Mesh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Movement
{
    internal class FpcMove
    {
        public Vector3 DesiredDirection { get; set; } = Vector3.zero;

        private Area currentArea;
        private Area goalArea;
        private List<Area> areasPath = new();
        private int nextPathIdx = 1;

        private readonly FpcBotPlayer botPlayer;

        public FpcMove(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public void ToPosition(Vector3 targetPosition)
        {
            var navMesh = NavigationMesh.Instance;
            var playerPosition = botPlayer.FpcRole.FpcModule.transform.position;

            var nearbyArea = navMesh.GetAreaWithin(playerPosition);
            var targetArea = navMesh.GetAreaWithin(targetPosition);

            if (targetArea != this.goalArea || nearbyArea != this.currentArea)
            {
                this.currentArea = nearbyArea;
                this.goalArea = targetArea;

                this.areasPath = navMesh.GetShortestPath(this.currentArea, this.goalArea);
                this.nextPathIdx = 1;
            }

            var currentPosition = this.currentArea.CenterPosition;
            DesiredDirection = Vector3.Normalize(currentPosition - playerPosition);

            if (Vector3.Distance(playerPosition, this.currentArea.CenterPosition) < 1f)
            {
                this.currentArea = this.areasPath.Count > this.nextPathIdx ? this.areasPath[this.nextPathIdx++] : null;
            }
        }

        public IEnumerator<float> ToFpcAsync(Vector3 localDirection, int timeAmount)
        {
            var transform = botPlayer.FpcRole.FpcModule.transform;

            DesiredDirection = transform.TransformDirection(localDirection);

            yield return Timing.WaitForSeconds(timeAmount);

            DesiredDirection = Vector3.zero;

            yield break;
        }
    }
}
