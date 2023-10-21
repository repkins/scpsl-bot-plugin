using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.Usables;
using PluginAPI.Core.Zones;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.World;
using SCPSLBot.Navigation.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities
{
    internal class Explore : IActivity
    {
        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<ItemWithinSight<KeycardPickup>>(this);
            //fpcMind.ActivityImpacts<ItemWithinSightMedkit>(this);
            //fpcMind.ActivityImpacts<ItemWithinSight<FirearmPickup>>(this);
        }

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {

        }

        public Func<bool> Condition => () => true;

        public Explore(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public void Tick()
        {
            var position = this.botPlayer.FpcRole.FpcModule.Position;
            var nodeGraph = NavigationGraph.Instance;

            // Set new position leading to unexplored area (room)
            // 1. Select (any) open node within front in adjacent rooms and trace route.
            // 2. Move character to selected node by following traced route.
            // 3. When characted reached selected open node, start from 1.

            if (this.node is null)
            {
                this.node = nodeGraph.FindNearestNode(position, 5f);

                var possibleGoalNodes = nodeGraph.NodesByRoom[this.node.Room]
                    .Where(n => n.ForeignNodes.Any() && n == this.node)
                    .Select(n => n.ForeignNodes.First());
                this.goalNode = possibleGoalNodes.First(fn => UnityEngine.Random.value > 0.5f);

                this.path = nodeGraph.GetShortestPath(this.node, this.goalNode);
            }

            this.botPlayer.DesiredMoveDirection = Vector3.Normalize(this.node.Position - position);

            if (Vector3.Distance(position, this.node.Position) < 1f)
            {
                this.node = this.path.Count > this.nextPathIdx ? this.path[this.nextPathIdx++] : null;
            }
        }

        private readonly FpcBotPlayer botPlayer;

        private Node node;
        private Node goalNode;
        private List<Node> path = new ();
        private int nextPathIdx = 1;
    }
}
