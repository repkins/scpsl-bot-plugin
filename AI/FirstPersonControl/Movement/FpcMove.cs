using MEC;
using SCPSLBot.Navigation.Graph;
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

        private Node currentNode;
        private Node goalNode;
        private List<Node> nodesPath = new();
        private int nextPathIdx = 1;

        private readonly FpcBotPlayer botPlayer;

        public FpcMove(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public void ToPosition(Vector3 targetPosition)
        {
            var nodeGraph = NavigationGraph.Instance;
            var playerPosition = botPlayer.FpcRole.FpcModule.transform.position;

            var nearbyNode = nodeGraph.FindNearestNode(playerPosition, 5f);
            var targetNode = nodeGraph.FindNearestNode(targetPosition, 5f);

            if (targetNode != this.goalNode || nearbyNode != this.currentNode)
            {
                this.currentNode = nearbyNode;
                this.goalNode = targetNode;

                this.nodesPath = nodeGraph.GetShortestPath(this.currentNode, this.goalNode);
                this.nextPathIdx = 1;
            }

            var currentPosition = this.currentNode.Position;
            DesiredDirection = Vector3.Normalize(currentPosition - playerPosition);

            if (Vector3.Distance(playerPosition, this.currentNode.Position) < 1f)
            {
                this.currentNode = this.nodesPath.Count > this.nextPathIdx ? this.nodesPath[this.nextPathIdx++] : null;
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
