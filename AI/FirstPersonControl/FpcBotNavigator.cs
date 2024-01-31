﻿using PluginAPI.Core;
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

        private readonly NavigationMesh navMesh = NavigationMesh.Instance;

        private readonly FpcBotPlayer botPlayer;

        public FpcBotNavigator(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public Vector3 GetPositionTowards(Vector3 goalPosition)
        {
            this.UpdateNavigationTo(goalPosition);

            if (!IsAtLastArea())
            {
                Vector3 nextTargetPosition = GetNextCorner(goalPosition);
                return nextTargetPosition;
            }
            else 
            {
                // Target position should be on current area.
                return goalPosition;
            }
        }

        public IEnumerable<Vector3> GetPathTowards(Vector3 goalPosition)
        {
            this.UpdateNavigationTo(goalPosition);

            var playerPosition = botPlayer.FpcRole.FpcModule.transform.position;

            var points = botPlayer.Navigator.AreasPath.Zip(botPlayer.Navigator.AreasPath.Skip(1), (area, nextArea) => (area, nextArea))
                    .Select(t => t.area.ConnectedAreaEdges[t.nextArea])
                    .Select(e => Vector3.Lerp(e.From.Position, e.To.Position, .5f))
                    .Prepend(playerPosition)
                    .Append(goalPosition);

            return points;
        }

        private void UpdateNavigationTo(Vector3 goalPosition)
        {
            var playerPosition = botPlayer.FpcRole.FpcModule.transform.position;

            if (!IsAtLastArea())
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
                while (isEdgeReached && !IsAtLastArea());
            }

            var withinArea = navMesh.GetAreaWithin(playerPosition);
            var targetArea = navMesh.GetAreaWithin(goalPosition);

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
        }

        private Vector3 GetNextCorner(Vector3 goalPosition)
        {
            var playerPosition = botPlayer.FpcRole.FpcModule.transform.position;

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

            return nextTargetPosition;
        }

        private bool IsAtLastArea()
        {
            return this.currentPathIdx >= this.AreasPath.Count - 1;
        }
    }
}