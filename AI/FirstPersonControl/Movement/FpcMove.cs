using MEC;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.World;
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
        //public Vector3 DesiredDirection { get; set; } = Vector3.zero;
        public Vector3 DesiredLocalDirection { get; set; } = Vector3.zero;

        private Area currentArea;
        private Area goalArea;
        public List<Area> AreasPath = new();
        private int currentPathIdx = -1;

        private readonly FpcBotPlayer botPlayer;

        public FpcMove(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public void ForwardToPosition(Vector3 targetPosition)
        {
            var navMesh = NavigationMesh.Instance;
            var playerPosition = botPlayer.FpcRole.FpcModule.transform.position;

            bool isAtLastArea() => this.currentPathIdx >= this.AreasPath.Count - 1;
            if (!isAtLastArea())
            {
                bool isEdgeReached;
                do
                {
                    var nextTargetArea = this.AreasPath[this.currentPathIdx + 1];
                    var nextTargetAreaEdge = currentArea.ConnectedAreaEdges[nextTargetArea];

                    isEdgeReached = navMesh.IsAtPositiveEdgeSide(playerPosition, nextTargetAreaEdge);
                    if (isEdgeReached)
                    {
                        this.currentArea = this.AreasPath[++this.currentPathIdx];
                        Log.Debug($"New current area {this.currentArea}.");
                    }
                }
                while (isEdgeReached && !isAtLastArea());
            }

            if (!isAtLastArea())
            {
                var nextTargetArea = this.AreasPath[this.currentPathIdx + 1];
                var nextTargetAreaEdge = currentArea.ConnectedAreaEdges[nextTargetArea];
                var nextTargetPosition = Vector3.Lerp(nextTargetAreaEdge.From.Position, nextTargetAreaEdge.To.Position, 0.5f);

                var relativePos = nextTargetPosition - botPlayer.FpcRole.CameraPosition;
                var relativeProjected = Vector3.ProjectOnPlane(relativePos, Vector3.up);
                var nextTargetLookPosition = relativeProjected + botPlayer.FpcRole.CameraPosition;

                botPlayer.Look.ToPosition(nextTargetLookPosition);

                DesiredLocalDirection = Vector3.forward;
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

                this.AreasPath = navMesh.GetShortestPath(this.currentArea, this.goalArea);
                this.currentPathIdx = 0;

                Log.Debug($"New path of {this.AreasPath.Count} areas:");
                foreach (var areaInPath in AreasPath)
                {
                    Log.Debug($"Area {areaInPath}.");
                }
            }

            if (isAtLastArea())
            {
                // Target position should be on current area.
                var relativePos = targetPosition - botPlayer.FpcRole.CameraPosition;
                var relativeProjected = Vector3.ProjectOnPlane(relativePos, Vector3.up);
                var targetLookPosition = relativeProjected + botPlayer.FpcRole.CameraPosition;

                botPlayer.Look.ToPosition(targetLookPosition);

                DesiredLocalDirection = Vector3.forward;
            }
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
