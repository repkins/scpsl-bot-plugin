using Interactables;
using Interactables.Interobjects.DoorUtils;
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

        private const float interactDistance = 2f;

        public void Tick()
        {
            var playerPosition = botPlayer.FpcRole.FpcModule.Position;
            var navMesh = NavigationMesh.Instance;

            // Set new position leading to unexplored area (room)
            // 1. Select (any) open area within front in adjacent rooms and trace route.
            // 2. Move character to selected area by following traced route.
            // 3. When characted reached selected open area, start from 1.

            var withinArea = navMesh.GetAreaWithin(playerPosition);

            if (goalArea is not null && goalArea == withinArea)
            {
                goalArea = null;
            }

            if (goalArea is null && withinArea is not null)
            {
                var areasWithForeign = navMesh.AreasByRoom[withinArea.Room]
                    .Where(a => a.ForeignConnectedAreas.Any());

                var selectedAreas = areasWithForeign.Take(2)
                    .Count() < 2
                        ? areasWithForeign
                        : areasWithForeign.Where(awf => awf != withinArea);

                var possibleGoalAreas = selectedAreas
                    .SelectMany(a => a.ForeignConnectedAreas)
                    .ToArray();

                var goalIdx = UnityEngine.Random.Range(0, possibleGoalAreas.Length);
                goalArea = possibleGoalAreas[goalIdx];
            }

            if (goalArea is not null)
            {
                var points = botPlayer.Navigator.GetPathTowards(goalArea.CenterPosition);
                var doorsOnPath = botPlayer.Perception.GetDoorsOnPath(points);

                var firstDoorOnPath = doorsOnPath.FirstOrDefault();
                if (firstDoorOnPath && Vector3.Distance(firstDoorOnPath.transform.position + Vector3.up, playerPosition) <= interactDistance)
                {
                    Log.Debug($"{firstDoorOnPath} is within interactable distance");

                    var hub = botPlayer.BotHub.PlayerHub;

                    if (!firstDoorOnPath.TargetState && !botPlayer.OpenDoor(firstDoorOnPath, interactDistance))
                    {
                        botPlayer.LookToPosition(firstDoorOnPath.transform.position);
                        //Log.Debug($"Looking towards door interactable");
                    }
                }
                else
                {
                    botPlayer.MoveToPosition(goalArea.CenterPosition);
                }
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
