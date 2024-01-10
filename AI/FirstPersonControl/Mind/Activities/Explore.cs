using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.Usables;
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
            var navMesh = NavigationMesh.Instance;

            // Set new position leading to unexplored area (room)
            // 1. Select (any) open area within front in adjacent rooms and trace route.
            // 2. Move character to selected area by following traced route.
            // 3. When characted reached selected open area, start from 1.

            if (goalArea is not null && Vector3.Distance(position, goalArea.CenterPosition) < 1f)
            {
                goalArea = null;
            }

            if (goalArea is null)
            {
                var nearbyArea = navMesh.GetAreaWithin(position);

                var possibleGoalAreas = navMesh.AreasByRoom[nearbyArea.Room]
                    .Where(n => n.ForeignConnectedAreas.Any() && n != nearbyArea)
                    .Select(n => n.ForeignConnectedAreas.First());
                goalArea = possibleGoalAreas.First(fn => UnityEngine.Random.value > 0.5f);
            }

            botPlayer.MoveToPosition(goalArea.CenterPosition);
        }

        public void Reset()
        {
            goalArea = null;
        }

        private readonly FpcBotPlayer botPlayer;

        private Area goalArea;
    }
}
