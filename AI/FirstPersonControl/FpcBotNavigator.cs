using PluginAPI.Core;
using SCPSLBot.Navigation.Mesh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl
{
    internal class FpcBotNavigator
    {
        private Area currentArea;
        private Area goalArea;
        public List<Area> AreasPath = new();
        private int currentPathIdx = -1;

        private readonly FpcBotPlayer botPlayer;

        public FpcBotNavigator(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public Vector3 GetPositionTowards(Vector3 goalPosition)
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
                var nextTargetEdgeMiddlePosition = Vector3.Lerp(nextTargetAreaEdge.From.Position, nextTargetAreaEdge.To.Position, 0.5f);

                var nextTargetPosition = nextTargetEdgeMiddlePosition;

                var aheadPathIdx = this.currentPathIdx + 1;

                while (nextTargetEdgeMiddlePosition == nextTargetPosition && aheadPathIdx < this.AreasPath.Count - 1)
                {
                    aheadPathIdx++;

                    var relNextTargetEdgePos = (
                        from: nextTargetAreaEdge.From.Position - playerPosition,
                        to: nextTargetAreaEdge.To.Position - playerPosition);

                    var aheadTargetArea = this.AreasPath[aheadPathIdx];
                    var aheadTargetAreaEdge = nextTargetArea.ConnectedAreaEdges[aheadTargetArea];

                    var relAheadTargetEdgePos = (
                        from: aheadTargetAreaEdge.From.Position - playerPosition,
                        to: aheadTargetAreaEdge.To.Position - playerPosition);

                    var dirToAheadTargetEdgeNormals = (
                        from: Vector3.Cross(relAheadTargetEdgePos.from, Vector3.up),
                        to: Vector3.Cross(relAheadTargetEdgePos.to, Vector3.up));

                    if (Vector3.Dot(relNextTargetEdgePos.from, dirToAheadTargetEdgeNormals.from) > 0)
                    {
                        nextTargetPosition = nextTargetAreaEdge.From.Position;
                    }

                    if (Vector3.Dot(relNextTargetEdgePos.to, dirToAheadTargetEdgeNormals.to) < 0)
                    {
                        nextTargetPosition = nextTargetAreaEdge.To.Position;
                    }

                    nextTargetArea = aheadTargetArea;
                    nextTargetAreaEdge = aheadTargetAreaEdge;
                }

                if (nextTargetPosition == nextTargetEdgeMiddlePosition)
                {
                    nextTargetPosition = goalPosition;

                    var relNextTargetEdgePos = (
                        from: nextTargetAreaEdge.From.Position - playerPosition,
                        to: nextTargetAreaEdge.To.Position - playerPosition);

                    var relGoalPos = goalPosition - playerPosition;
                    var dirToGoalNormal = Vector3.Cross(relGoalPos, Vector3.up);

                    if (Vector3.Dot(relNextTargetEdgePos.from, dirToGoalNormal) > 0)
                    {
                        nextTargetPosition = nextTargetAreaEdge.From.Position;
                    }

                    if (Vector3.Dot(relNextTargetEdgePos.to, dirToGoalNormal) < 0)
                    {
                        nextTargetPosition = nextTargetAreaEdge.To.Position;
                    }
                }

            }

            var withinArea = navMesh.GetAreaWithin(playerPosition);
            var targetArea = navMesh.GetAreaWithin(goalPosition);

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
                var relativePos = goalPosition - botPlayer.FpcRole.CameraPosition;
                var relativeProjected = Vector3.ProjectOnPlane(relativePos, Vector3.up);
                var targetLookPosition = relativeProjected + botPlayer.FpcRole.CameraPosition;

                return targetLookPosition;
            }

            Log.Warning($"Could not able to resolve target position.");

            return playerPosition;
        }
    }
}
