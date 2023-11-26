using MEC;
using SCPSLBot.Navigation.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl
{
    internal partial class FpcBotPlayer
    {
        public Vector3 DesiredMoveDirection { get; set; } = Vector3.zero;

        private Node currentNode;
        private Node goalNode;
        private List<Node> nodesPath = new();
        private int nextPathIdx = 1;

        public void MoveToPosition(Vector3 targetPosition)
        {
            var nodeGraph = NavigationGraph.Instance;
            var playerPosition = FpcRole.FpcModule.transform.position;

            var nearbyNode = nodeGraph.FindNearestNode(playerPosition, 5f);
            var targetNode = nodeGraph.FindNearestNode(targetPosition, 5f);
            if (targetNode != this.goalNode || nearbyNode != this.currentNode)
            {
                this.currentNode = nearbyNode;
                this.goalNode = targetNode;

                this.nodesPath = nodeGraph.GetShortestPath(this.currentNode, this.goalNode);
            }

            DesiredMoveDirection = Vector3.Normalize(this.currentNode.Position - playerPosition);

            if (Vector3.Distance(playerPosition, this.currentNode.Position) < 1f)
            {
                this.currentNode = this.nodesPath.Count > this.nextPathIdx ? this.nodesPath[this.nextPathIdx++] : null;
            }
        }

        public IEnumerator<float> MoveFpcAsync(Vector3 localDirection, int timeAmount)
        {
            var transform = FpcRole.FpcModule.transform;

            DesiredMoveDirection = transform.TransformDirection(localDirection);

            yield return Timing.WaitForSeconds(timeAmount);

            DesiredMoveDirection = Vector3.zero;

            yield break;
        }
    }
}
