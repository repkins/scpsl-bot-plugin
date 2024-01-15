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

                var selectedAreas = areasWithForeign.Take(2).Count() < 2 ? areasWithForeign : areasWithForeign.Where(awf => awf != withinArea);
                var possibleGoalAreas = selectedAreas
                    .SelectMany(a => a.ForeignConnectedAreas)
                    .ToArray();

                var goalIdx = UnityEngine.Random.Range(0, possibleGoalAreas.Length);
                goalArea = possibleGoalAreas[goalIdx];
            }

            if (goalArea is not null)
            {
                botPlayer.MoveForwardToPosition(goalArea.CenterPosition);

                var doorsWithinSight = this.botPlayer.Perception.DoorsWithinSight;

                var points = botPlayer.Move.AreasPath.Zip(botPlayer.Move.AreasPath.Skip(1), (area, nextArea) => (area, nextArea))
                    .Select(t => t.area.ConnectedAreaEdges[t.nextArea])
                    .Select(e => Vector3.Lerp(e.From.Position, e.To.Position, .5f))
                    .Prepend(playerPosition)
                    .Append(goalArea.CenterPosition);

                var rays = points.Zip(points.Skip(1), (point, nextPoint) => new Ray(point, nextPoint - point));

                var doorsOnPath = rays
                    .Select(ray => doorsWithinSight
                        .FirstOrDefault(door => door.GetComponentsInChildren<Collider>()
                            .Any(collider => collider.Raycast(ray, out _, 1f))))
                    .Where(d => d != null);

                var firstDoorOnPath = doorsOnPath.FirstOrDefault();
                if (firstDoorOnPath)
                {
                    var dist = Vector3.Distance(firstDoorOnPath.transform.position + Vector3.up, playerPosition);
                    //Log.Debug($"First door on path {firstDoorOnPath} with state {firstDoorOnPath.TargetState} and dist {dist}");

                    if (dist <= interactDistance)
                    {
                        Log.Debug($"{firstDoorOnPath} is within interactable distance");

                        var hub = botPlayer.BotHub.PlayerHub;

                        //if (firstDoorOnPath.GetComponentsInChildren<Collider>()
                        //        .Any(collider => collider.Raycast(new Ray(playerPosition, hub.PlayerCameraReference.forward), out var hit, 1f))
                        if (Physics.Raycast(playerPosition, hub.PlayerCameraReference.forward, out var hit, interactDistance, LayerMask.GetMask("Door"))
                            && hit.collider.GetComponent<InteractableCollider>() is InteractableCollider interactableCollider
                            && hit.collider.GetComponentInParent<IServerInteractable>() is IServerInteractable interactable)
                        {
                            var colliderId = interactableCollider.ColliderId;

                            interactable.ServerInteract(hub, colliderId);
                            //Log.Debug($"ServerInteract(...) called on {interactable}");
                        }
                        else
                        {
                            botPlayer.LookToPosition(firstDoorOnPath.transform.position);
                            //Log.Debug($"Looking towards door interactable");
                        }
                    }
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
