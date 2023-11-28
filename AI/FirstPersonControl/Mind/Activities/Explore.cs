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

        public bool Condition() => true;

        public Explore(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public void Tick()
        {
            var position = botPlayer.FpcRole.FpcModule.Position;
            var nodeGraph = NavigationGraph.Instance;

            // Set new position leading to unexplored area (room)
            // 1. Select (any) open node within front in adjacent rooms and trace route.
            // 2. Move character to selected node by following traced route.
            // 3. When characted reached selected open node, start from 1.

            if (goalNode is not null && Vector3.Distance(position, goalNode.Position) < 1f)
            {
                goalNode = null;
            }

            if (goalNode is null)
            {
                var nearbyNode = nodeGraph.FindNearestNode(position, 5f);

                var possibleGoalNodes = nodeGraph.NodesByRoom[nearbyNode.Room]
                    .Where(n => n.ForeignNodes.Any() && n != nearbyNode)
                    .Select(n => n.ForeignNodes.First());
                goalNode = possibleGoalNodes.First(fn => UnityEngine.Random.value > 0.5f);
            }

            botPlayer.MoveToPosition(goalNode.Position);
        }

        private readonly FpcBotPlayer botPlayer;
        private Node goalNode;
    }
}
