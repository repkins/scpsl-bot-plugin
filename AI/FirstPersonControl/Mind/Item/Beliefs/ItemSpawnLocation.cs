using System;
using UnityEngine;
using MapGeneration;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using PluginAPI.Core;
using MapGeneration.Distributors;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs
{
    internal class ItemSpawnLocation<C> : IBelief where C : IItemBeliefCriteria
    {
        public C Criteria { get; }
        public ItemSpawnLocation(C criteria, ItemType[] spawnItemTypes, RoomSightSense roomSense, ItemsWithinSightSense itemsSightSense) 
            : this(spawnItemTypes, roomSense, itemsSightSense)
        {
            this.Criteria = criteria;
        }

        private readonly ItemType[] spawnItemTypes;
        private readonly RoomSightSense roomSense;
        private readonly ItemsWithinSightSense itemsSightSense;

        private ItemSpawnLocation(ItemType[] spawnItemTypes, RoomSightSense roomSense, ItemsWithinSightSense itemsSightSense)
        {
            this.spawnItemTypes = spawnItemTypes;
            this.roomSense = roomSense;
            this.itemsSightSense = itemsSightSense;

            this.roomSense.OnAfterSensedForeignRooms += OnAfterSensedForeignRooms;
            this.itemsSightSense.OnAfterSensedItemsWithinSight += OnAfterSensedItemsWithinSight;
        }

        private readonly HashSet<Vector3> visitedSpawnPositions = new();

        private void OnAfterSensedItemsWithinSight()
        {
            if (this.Position.HasValue)
            {
                var spawnPosition = this.Position.Value;

                if (this.itemsSightSense.IsPositionWithinFov(spawnPosition)
                    && (!itemsSightSense.IsPositionObstructed(Position.Value) || itemsSightSense.GetDistanceToPosition(Position.Value) < 1.5f))
                {
                    this.visitedSpawnPositions.Add(spawnPosition);
                    Update(null);
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

            //if (!this.Position.HasValue)
            //{
                var unvisitedSpawnPosition = this.GetItemSpawnPositions(roomWithin)
                    .Where(spawnPosition => !this.visitedSpawnPositions.Contains(spawnPosition))
                    .Select(spawnPosition => new Vector3?(spawnPosition))
                    .FirstOrDefault();

                if (unvisitedSpawnPosition.HasValue)
                {
                    this.Update(unvisitedSpawnPosition.Value);
                }
            //}
        }

        private readonly Dictionary<RoomIdentifier, Vector3[]> roomItemSpawnPositions = new();
        private Vector3[] GetItemSpawnPositions(RoomIdentifier room)
        {
            if (!this.roomItemSpawnPositions.TryGetValue(room, out var spawnPositions))
            {
                spawnPositions = room.GetComponentsInChildren<ItemSpawnpoint>()
                    .Where(spawnPoint => this.spawnItemTypes.Any(spawnItemType => spawnPoint.InAcceptedItems(spawnItemType)))
                    .Select(spawnPoint => spawnPoint.transform.position)
                    .ToArray();
            }
            return spawnPositions;
        }

        public Vector3? Position { get; private set; }

        public event Action OnUpdate;

        private void Update(Vector3? spawnPosition)
        {
            if (spawnPosition != this.Position)
            {
                this.Position = spawnPosition;
                this.OnUpdate?.Invoke();
            }
        }

        public override string ToString()
        {
            return $"{nameof(ItemSpawnLocation<C>)}({this.Criteria}): {this.Position}";
        }
    }

    internal static class ItemSpawnpointExtensions
    {
        private static readonly FieldInfo acceptedItemsField = AccessTools.DeclaredField(typeof(ItemSpawnpoint), "_acceptedItems");
        public static bool InAcceptedItems(this ItemSpawnpoint spawnpoint, ItemType itemType)
        {
            var acceptedItems = acceptedItemsField.GetValue(spawnpoint) as ItemType[];
            return acceptedItems.Any(i => i == itemType);
        }
    }
}
