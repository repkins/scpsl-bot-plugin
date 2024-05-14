using Interactables;
using MapGeneration;
using MapGeneration.Distributors;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs
{
    internal record struct LockerItemSpawn(Vector3 Position, Vector3 LockerDirection, bool LockerOpened);

    internal class ItemInLocker<C> : ItemLocation<C> where C : IItemBeliefCriteria
    {
        private readonly ItemType[] spawnItemTypes;
        private readonly RoomSightSense roomSense;
        private readonly ItemsWithinSightSense itemsSightSense;

        public ItemInLocker(C criteria, ItemType[] spawnItemTypes, RoomSightSense roomSense, ItemsWithinSightSense itemsSightSense, FpcBotNavigator navigator) 
            : base(criteria, navigator, itemsSightSense)
        {
            this.spawnItemTypes = spawnItemTypes;
            this.itemsSightSense = itemsSightSense;
            this.roomSense = roomSense;

            this.roomSense.OnAfterSensedForeignRooms += OnAfterSensedForeignRooms;
            this.itemsSightSense.OnAfterSensedItemsWithinSight += OnAfterSensedItemsWithinSight;
        }

        public Vector3? LockerDirection { get; private set; }
        public bool? LockerOpened { get; private set; }
        public InteractableCollider LockerDoor { get; private set; }

        private readonly HashSet<Vector3> visitedSpawnPositions = new();

        private void OnAfterSensedItemsWithinSight()
        {
            if (this.AccessiblePosition.HasValue)
            {
                var spawnPosition = this.AccessiblePosition.Value;

                if (this.itemsSightSense.IsPositionWithinFov(spawnPosition)
                //    && (!itemsSightSense.IsPositionObstructed(Position.Value) || itemsSightSense.GetDistanceToPosition(Position.Value) < 1.5f))
                    && (!itemsSightSense.IsPositionObstructed(spawnPosition)))
                {
                    this.visitedSpawnPositions.Add(spawnPosition);
                    ClearPosition();
                    LockerDirection = null;
                    LockerOpened = null;
                }
            }
        }

        private void OnAfterSensedForeignRooms()
        {
            var roomWithin = this.roomSense.RoomWithin;
            if (roomWithin == null)
            {
                Log.Debug($"RoomSightSense.RoomWithin is null");
                return;
            }

            var unvisitedLockerChamber = this.GetItemSpawns(roomWithin)
                .Where(itemSpawn => !this.visitedSpawnPositions.Contains(itemSpawn.transform.position))
                .Where(itemSpawn => this.IsAccessible(itemSpawn.transform.position))
                .FirstOrDefault();

            if (unvisitedLockerChamber)
            {
                this.SetAccesablePosition(unvisitedLockerChamber.transform.position);
                this.LockerDirection = unvisitedLockerChamber.transform.forward;
                this.LockerOpened = unvisitedLockerChamber.IsOpen;
                this.LockerDoor = unvisitedLockerChamber.GetComponent<InteractableCollider>();
            }
        }

        private readonly Dictionary<RoomIdentifier, LockerChamber[]> roomLockerChambers = new();

        private LockerChamber[] GetItemSpawns(RoomIdentifier room)
        {
            // TODO: remove when remaining nav mesh added in room
            if (room.Name == RoomName.LczGreenhouse)
            {
                return Array.Empty<LockerChamber>();
            }

            if (!this.roomLockerChambers.TryGetValue(room, out var lockerChambers))
            {
                lockerChambers = room.GetComponentsInChildren<LockerChamber>()
                    .Where(lockerChamber => this.spawnItemTypes.Any(spawnItemType => lockerChamber.AcceptableItems.Contains(spawnItemType)))
                    .ToArray();
            }
            return lockerChambers;
        }

        public override string ToString()
        {
            return $"{nameof(ItemInLocker<C>)}({this.Criteria}): {this.AccessiblePosition}";
        }
    }
}
