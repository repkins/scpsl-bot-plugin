using MEC;
using PluginAPI.Core;
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
        private int currentPathIdx = -1;

        private readonly FpcBotPlayer botPlayer;

        public FpcMove(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public void ToPosition(Vector3 targetPosition)
        {
            var navMesh = NavigationMesh.Instance;
            var playerPosition = botPlayer.FpcRole.FpcModule.transform.position;

            bool isAtLastArea() => this.currentPathIdx >= this.areasPath.Count - 1;
            if (!isAtLastArea())
            {
                bool isEdgeReached;
                do
                {
                    var nextTargetArea = this.areasPath[this.currentPathIdx + 1];
                    var nextTargetAreaEdge = currentArea.ConnectedAreaEdges[nextTargetArea];

                    isEdgeReached = navMesh.IsAtPositiveEdgeSide(playerPosition, nextTargetAreaEdge);
                    if (isEdgeReached)
                    {
                        this.currentArea = this.areasPath[++this.currentPathIdx];
                        Log.Debug($"New current area {this.currentArea}.");
                    }
                }
                while (isEdgeReached && !isAtLastArea());
            }

            if (!isAtLastArea())
            {
                var nextTargetArea = this.areasPath[this.currentPathIdx + 1];
                var nextTargetAreaEdge = currentArea.ConnectedAreaEdges[nextTargetArea];
                var nextTargetPosition = Vector3.Lerp(nextTargetAreaEdge.From.Position, nextTargetAreaEdge.To.Position, 0.5f);

                DesiredDirection = Vector3.Normalize(nextTargetPosition - playerPosition);

                //Log.Debug($"Next target area edge {nextTargetAreaEdge}.");
            }

            var withinArea = navMesh.GetAreaWithin(playerPosition);
            var targetArea = navMesh.GetAreaWithin(targetPosition);

            //Log.Debug($"Within area {withinArea}.");

            if (withinArea != null && (targetArea != this.goalArea || withinArea != this.currentArea))
            {
                this.currentArea = withinArea;
                this.goalArea = targetArea;
                Log.Debug($"New start area {withinArea}.");
                Log.Debug($"New goal area {targetArea}.");

                this.areasPath = navMesh.GetShortestPath(this.currentArea, this.goalArea);
                this.currentPathIdx = 0;

                Log.Debug($"New path of {this.areasPath.Count} areas:");
                foreach (var areaInPath in areasPath)
                {
                    Log.Debug($"Area {areaInPath}.");
                }
            }

            if (isAtLastArea())
            {
                // Target position should be on current area.
                DesiredDirection = Vector3.Normalize(targetPosition - playerPosition);
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
