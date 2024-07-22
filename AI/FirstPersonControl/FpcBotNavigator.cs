using MapGeneration;
using PluginAPI.Core;
using SCPSLBot.Navigation.Mesh;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl
{
    internal class FpcBotNavigator
    {
        private Vector3 lastPlayerPosition;
        private Area areaWithin;

        private Area currentArea;
        private Area goalArea;
        public List<Area> AreasPath { get; } = new();
        public IEnumerable<(Area Area, Area NextArea)> AreaPathSegments { get; }
        private int currentPathIdx = -1;

        public Vector3 GoalPosition { get; private set; }
        public List<Vector3> PointsPath { get; } = new();
        public IEnumerable<(Vector3 point, Vector3 nextPoint)> PathSegments { get; }

        private bool isGoalOutside;
        private Vector3 targetAreaClosestPositionToGoal;
        
        private readonly NavigationMesh navMesh = NavigationMesh.Instance;

        private readonly FpcBotPlayer botPlayer;

        public FpcBotNavigator(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;

            this.PathSegments = PointsPath.Zip(PointsPath.Skip(1), (point, nextPoint) => (point, nextPoint));
            this.AreaPathSegments = AreasPath.Zip(AreasPath.Skip(1), (area, nextArea) => (area, nextArea));
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
                if (goalArea != null && isGoalOutside)
                {
                    return targetAreaClosestPositionToGoal;
                }

                return goalPosition;
            }
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
                        //Log.Debug($"New current area {this.currentArea}.");
                    }
                }
                while (isEdgeReached && !IsAtLastArea());
            }

            var withinArea = GetAreaWithin();
            var targetArea = navMesh.GetAreaWithin(goalPosition);

            if (targetArea == null)
            {
                var goalRoom = RoomIdUtils.RoomAtPositionRaycasts(goalPosition);

                var nearestEdge = navMesh.GetNearestEdge(goalPosition, out var closestPoint, goalRoom);
                if (nearestEdge.HasValue)
                {
                    var nearestRoomKindEdge = new RoomKindEdge(nearestEdge.Value.From.RoomKindVertex, nearestEdge.Value.To.RoomKindVertex);
                    targetArea = navMesh.AreasByRoom[goalRoom.ApiRoom].Find(a => a.RoomKindArea.Edges.Any(e => e == nearestRoomKindEdge));
                    targetAreaClosestPositionToGoal = closestPoint;
                }
                else
                {
                    Log.Warning($"Could not find path to goal position.");
                }

                isGoalOutside = true;
            }
            else
            {
                isGoalOutside = false;
            }

            if (withinArea != null && (targetArea != this.goalArea || withinArea != this.currentArea))
            {
                this.currentArea = withinArea;
                this.goalArea = targetArea;
                //Log.Debug($"New start area {withinArea}.");
                //Log.Debug($"New goal area {targetArea}.");

                navMesh.FindShortestPath(this.currentArea, this.goalArea, this.AreasPath);
                this.currentPathIdx = 0;

                //Log.Debug($"New path of {this.AreasPath.Count} areas:");
                //foreach (var areaInPath in AreasPath)
                //{
                //    Log.Debug($"Area {areaInPath}.");
                //}

                this.GoalPosition = goalPosition;

                this.PointsPath.Clear();
                this.PointsPath.Add(playerPosition);

                var partialPath = false;
                foreach (var (area, nextArea) in AreaPathSegments)
                {
                    if (!area.ConnectedAreaEdges.TryGetValue(nextArea, out var e))
                    {
                        partialPath = true;
                        break;
                    }
                    this.PointsPath.Add(Vector3.Lerp(e.From.Position, e.To.Position, .5f));
                }

                if (!partialPath) 
                {
                    this.PointsPath.Add(goalPosition);
                }
            }
        }

        public Area GetAreaWithin()
        {
            var playerPosition = botPlayer.PlayerPosition;
            if (playerPosition != lastPlayerPosition)
            {
                areaWithin = navMesh.GetAreaWithin(playerPosition);
                lastPlayerPosition = playerPosition;
            }

            return areaWithin;
        }

        private Vector3 GetNextCorner(Vector3 goalPosition)
        {
            var playerPosition = botPlayer.PlayerPosition;

            var nextTargetArea = this.AreasPath[this.currentPathIdx + 1];
            if (!currentArea.ConnectedAreaEdges.TryGetValue(nextTargetArea, out var targetAreaEdge))
            {
                return currentArea.CenterPosition;
            }
            var nextTargetEdgeMiddlePosition = Vector3.Lerp(targetAreaEdge.From.Position, targetAreaEdge.To.Position, 0.5f);

            var nextTargetPosition = nextTargetEdgeMiddlePosition;

            var aheadPathIdx = this.currentPathIdx + 1;

            while (nextTargetEdgeMiddlePosition == nextTargetPosition && aheadPathIdx < this.AreasPath.Count - 1)
            {
                aheadPathIdx++;

                var relTargetEdgePos = (
                    from: targetAreaEdge.From.Position - playerPosition,
                    to: targetAreaEdge.To.Position - playerPosition);

                var aheadTargetArea = this.AreasPath[aheadPathIdx];
                if (!nextTargetArea.ConnectedAreaEdges.TryGetValue(aheadTargetArea, out var aheadTargetAreaEdge))
                {
                    goalPosition = nextTargetArea.CenterPosition;
                    break;
                }

                var relAheadTargetEdgePos = (
                    from: aheadTargetAreaEdge.From.Position - playerPosition,
                    to: aheadTargetAreaEdge.To.Position - playerPosition);

                var dirToAheadTargetEdgeNormals = (
                    from: Vector3.Cross(relAheadTargetEdgePos.from, Vector3.up),
                    to: Vector3.Cross(relAheadTargetEdgePos.to, Vector3.up));

                if (Vector3.Dot(relTargetEdgePos.from, dirToAheadTargetEdgeNormals.from) < 0)
                {
                    targetAreaEdge.From = aheadTargetAreaEdge.From;
                }

                if (Vector3.Dot(relTargetEdgePos.to, dirToAheadTargetEdgeNormals.to) > 0)
                {
                    targetAreaEdge.To = aheadTargetAreaEdge.To;
                }


                if (Vector3.Dot(relTargetEdgePos.from, dirToAheadTargetEdgeNormals.to) > 0)
                {
                    nextTargetPosition = targetAreaEdge.From.Position;
                }

                if (Vector3.Dot(relTargetEdgePos.to, dirToAheadTargetEdgeNormals.from) < 0)
                {
                    nextTargetPosition = targetAreaEdge.To.Position;
                }

                nextTargetArea = aheadTargetArea;
            }

            if (nextTargetPosition == nextTargetEdgeMiddlePosition)
            {
                nextTargetPosition = goalPosition;

                var relNextTargetEdgePos = (
                    from: targetAreaEdge.From.Position - playerPosition,
                    to: targetAreaEdge.To.Position - playerPosition);

                var relGoalPos = goalPosition - playerPosition;
                var dirToGoalNormal = Vector3.Cross(relGoalPos, Vector3.up);

                if (Vector3.Dot(relNextTargetEdgePos.from, dirToGoalNormal) > 0)
                {
                    nextTargetPosition = targetAreaEdge.From.Position;
                }

                if (Vector3.Dot(relNextTargetEdgePos.to, dirToGoalNormal) < 0)
                {
                    nextTargetPosition = targetAreaEdge.To.Position;
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
