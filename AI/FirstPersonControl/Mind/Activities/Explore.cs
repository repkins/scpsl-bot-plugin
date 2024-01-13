using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.Usables;
using PluginAPI.Core;
using PluginAPI.Core.Zones;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.World;
using SCPSLBot.Navigation.Mesh;
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
        { }

        public bool Condition() => true;

        public Explore(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public void Tick()
        {
            var position = botPlayer.FpcRole.FpcModule.Position;
            var navMesh = NavigationMesh.Instance;

            // Set new position leading to unexplored area (room)
            // 1. Select (any) open area within front in adjacent rooms and trace route.
            // 2. Move character to selected area by following traced route.
            // 3. When characted reached selected open area, start from 1.

            var withinArea = navMesh.GetAreaWithin(position);

            if (goalArea is not null && goalArea == withinArea)
            {
                goalArea = null;
            }

            if (goalArea is null && withinArea is not null)
            {
                var possibleGoalAreas = navMesh.AreasByRoom[withinArea.Room]
                    .Where(n => n.ForeignConnectedAreas.Any() && n != withinArea)
                    .Select(n => n.ForeignConnectedAreas.First())
                    .ToArray();
                var goalIdx = UnityEngine.Random.Range(0, possibleGoalAreas.Length);
                goalArea = possibleGoalAreas[goalIdx];
            }

            if (goalArea is not null)
            {
                botPlayer.MoveToPosition(goalArea.CenterPosition);
                botPlayer.LookToMoveDirection();
            }
        }

        public void Reset()
        {
            goalArea = null;
        }

        private readonly FpcBotPlayer botPlayer;

        private Area goalArea;
    }
}
