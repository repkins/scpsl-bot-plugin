using Interactables;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.Usables;
using MapGeneration;
using MapGeneration.Distributors;
using PluginAPI.Core;
using PluginAPI.Core.Zones;
using PluginAPI.Core.Zones.Entrance;
using PluginAPI.Roles;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using SCPSLBot.MapGeneration;
using SCPSLBot.Navigation.Mesh;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            fpcMind.ActivityImpacts<ItemWithinSightKeycardO5>(this);
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
            var lockersWithinSightSense = botPlayer.Perception.GetSense<LockersWithinSightSense>();

            if (lockersWithinSightSense.LockersWithinSight.Any())
            {
                var lockerWithinSight = lockersWithinSightSense.LockersWithinSight.FirstOrDefault(l => l.StructureType == StructureType.StandardLocker);
                if (lockerWithinSight)
                {
                    var lockerPos = lockerWithinSight.transform.position;
                    var targetPlayerPosAtLocker = Vector3.ProjectOnPlane(lockerPos + lockerWithinSight.transform.forward * 2, Vector3.up);
                    targetPlayerPosAtLocker.y = playerPosition.y;

                    var closedChamber = lockerWithinSight.Chambers.FirstOrDefault(ch => !ch.IsOpen);
                    if (closedChamber)
                    {
                        if (Vector3.Distance(targetPlayerPosAtLocker, playerPosition) < 0.1f)
                        {
                            if (!botPlayer.OpenLockerDoor(closedChamber, interactDistance))
                            {
                                var posToChamber = closedChamber.GetComponentInChildren<InteractableCollider>().GetComponent<Collider>().bounds.center;

                                botPlayer.LookToPosition(posToChamber);
                                //Log.Debug($"Looking towards door interactable");
                            }
                            else
                            {
                                stopwatch.Restart();
                            }
                        }
                        else
                        {
                            botPlayer.MoveToPosition(targetPlayerPosAtLocker);
                        }

                        return;
                    }

                    if (stopwatch.Elapsed.TotalSeconds < 1f)
                    {
                        return;
                    }

                    stopwatch.Stop();
                }
            }

            var withinArea = navMesh.GetAreaWithin(playerPosition);

            if (withinArea != null && goalPoi.HasValue && Vector3.Distance(goalPoi.Value.pos, playerPosition) < 1f)
            {
                var idx = goalPoi.Value.idx;

                Log.Debug($"Reached goal poi, adding to visited pois with {(withinArea.RoomKindArea.RoomKind, idx)}");

                visitedPointsOfInterestIndices.Add((withinArea.RoomKindArea.RoomKind, idx));

                goalPoi = null;
            }

            if (withinArea != null && pointsOfInterests.TryGetValue(withinArea.RoomKindArea.RoomKind, out var roomPois))
            {
                var nextLocalPoi = roomPois
                    .Select((p, i) => (p, i))
                    .Where(t => !visitedPointsOfInterestIndices.Contains((withinArea.RoomKindArea.RoomKind, t.i)))
                    .Select(t => new (Vector3 p, int i)?(t))
                    .DefaultIfEmpty(null)
                    .First();

                if (nextLocalPoi.HasValue)
                {
                    goalPoi = (withinArea.Room.Transform.TransformPoint(nextLocalPoi.Value.p), nextLocalPoi.Value.i);
                }
            }

            // Set new position leading to unexplored area (room)
            // 1. Select (any) open area within front in adjacent rooms and trace route.
            // 2. Move character to selected area by following traced route.
            // 3. When characted reached selected open area, start from 1.

            if (goalArea is not null && goalArea == withinArea)
            {
                goalArea = null;
            }

            if (!goalPoi.HasValue && goalArea is null && withinArea is not null)
            {
                var areasWithForeign = navMesh.AreasByRoom[withinArea.Room]
                    .Where(a => a.ForeignConnectedAreas.Any());

                var selectedAreas = areasWithForeign.Take(2)
                    .Count() < 2
                        ? areasWithForeign
                        : areasWithForeign.Where(awf => Vector3.Dot(awf.CenterPosition - playerPosition, botPlayer.FpcRole.FpcModule.transform.forward) > 0f);

                var possibleGoalAreas = selectedAreas
                    .SelectMany(a => a.ForeignConnectedAreas)
                    //.Select(fa => fa.ConnectedAreas.First(ca => !fa.ForeignConnectedAreas.Contains(ca)))
                    .Select(fa => fa.ConnectedAreas.First())
                    .ToArray();

                var goalIdx = UnityEngine.Random.Range(0, possibleGoalAreas.Length);
                goalArea = possibleGoalAreas[goalIdx];
            }

            var goalPos = goalPoi?.pos ?? goalArea?.CenterPosition;

            if (goalPos.HasValue)
            {
                var points = botPlayer.Navigator.GetPathTowards(goalPos.Value);
                var doorsOnPath = botPlayer.Perception.GetDoorsOnPath(points);

                var firstDoorOnPath = doorsOnPath.FirstOrDefault();
                if (firstDoorOnPath && !firstDoorOnPath.TargetState && Vector3.Distance(firstDoorOnPath.transform.position + Vector3.up, playerPosition) <= interactDistance)
                {
                    Log.Debug($"{firstDoorOnPath} is within interactable distance");

                    if (!botPlayer.OpenDoor(firstDoorOnPath, interactDistance))
                    {
                        botPlayer.LookToPosition(firstDoorOnPath.transform.position + Vector3.up);
                        //Log.Debug($"Looking towards door interactable");
                    }
                }
                //else
                //{
                    botPlayer.MoveToPosition(goalPos.Value);
                //}
            }
        }

        public void Reset()
        {
            goalArea = null;
            goalPoi = null;
        }

        private readonly FpcBotPlayer botPlayer;

        private Area goalArea;
        private (Vector3 pos, int idx)? goalPoi;

        private Stopwatch stopwatch = new();

        private static Dictionary<(RoomName, RoomShape, RoomZone), List<Vector3>> pointsOfInterests = new()
        {
            // Could not find edge at the top of 173 chamber room (probably raycast for room gives null)
            { (RoomName.Lcz173, RoomShape.Endroom, RoomZone.LightContainment), new(){ new Vector3(8.02f, 12.43f, 6.94f) } },
            { (RoomName.LczGreenhouse, RoomShape.Straight, RoomZone.LightContainment), new(){ new Vector3(-0.07f, 0.96f, -4.62f) } },
            { (RoomName.LczGlassroom, RoomShape.Endroom, RoomZone.LightContainment), new(){ new Vector3(-0.51f, 0.96f, -1.51f) } },
            { (RoomName.LczToilets, RoomShape.Straight, RoomZone.LightContainment), new(){
                new Vector3(4.33f, 0.96f, -4.58f),
                new Vector3(-3.77f, 0.96f, -6.15f)
            } },
            { (RoomName.LczComputerRoom, RoomShape.Endroom, RoomZone.LightContainment), new(){
                new Vector3(-5.45f, 0.96f, 1.64f),
                new Vector3(5.50f, 0.96f, 0.13f)
            } },
            { (RoomName.Lcz330, RoomShape.Endroom, RoomZone.LightContainment), new(){ new Vector3(-5.99f, 0.96f, -1.35f) } },
        };

        private HashSet<((RoomName, RoomShape, RoomZone), int)> visitedPointsOfInterestIndices = new();
    }
}
