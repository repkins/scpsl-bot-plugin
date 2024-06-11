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
    internal class ItemSpawnLocation<C> : ItemLocation<C> where C : IItemBeliefCriteria
    {
        private readonly ItemType[] spawnItemTypes;
        private readonly RoomSightSense roomSense;
        private readonly ItemsWithinSightSense itemsSightSense;

        public ItemSpawnLocation(C criteria, ItemType[] spawnItemTypes, RoomSightSense roomSense, ItemsWithinSightSense itemsSightSense, FpcBotNavigator navigator) 
            : base(criteria, navigator, itemsSightSense)
        {
            this.spawnItemTypes = spawnItemTypes;
            this.itemsSightSense = itemsSightSense;
            this.roomSense = roomSense;

            this.roomSense.OnAfterSensedForeignRooms += OnAfterSensedForeignRooms;
            this.itemsSightSense.OnAfterSensedItemsWithinSight += OnAfterSensedItemsWithinSight;
        }

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
                    .Where(spawnPosition => this.IsAccessible(spawnPosition))
                    .Select(spawnPosition => new Vector3?(spawnPosition))
                    .FirstOrDefault();

                if (unvisitedSpawnPosition.HasValue)
                {
                    this.SetAccesablePosition(unvisitedSpawnPosition.Value);
                }
            //}
        }

        private readonly Dictionary<RoomIdentifier, Vector3[]> roomItemSpawnPositions = new();

        private Vector3[] GetItemSpawnPositions(RoomIdentifier room)
        {
            // TODO: remove when remaining nav mesh added in room
            if (room.Name == RoomName.LczGreenhouse)
            {
                return Array.Empty<Vector3>();
            }

            if (!this.roomItemSpawnPositions.TryGetValue(room, out var spawnPositions))
            {
                spawnPositions = room.GetComponentsInChildren<ItemSpawnpoint>()
                    .Where(spawnPoint => this.spawnItemTypes.Any(spawnItemType => spawnPoint.InAcceptedItems(spawnItemType)))
                    //.Where(spawnPoint => this.spawnItemTypes.Any(spawnItemType => spawnPoint.AutospawnItem == spawnItemType || spawnPoint.InAcceptedItems(spawnItemType)))
                    .SelectMany(spawnPoint => spawnPoint.GetPositionVariants())
                    .Select(positionVariant => positionVariant.position)
                    .ToArray();

                this.roomItemSpawnPositions.Add(room, spawnPositions);
            }

            return spawnPositions;
        }

        public override string ToString()
        {
            return $"{nameof(ItemSpawnLocation<C>)}({this.Criteria}): {this.AccessiblePosition}";
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

        private static readonly FieldInfo positionVariantsField = AccessTools.DeclaredField(typeof(ItemSpawnpoint), "_positionVariants");
        public static Transform[] GetPositionVariants(this ItemSpawnpoint spawnpoint)
        {
            var positionVariants = positionVariantsField.GetValue(spawnpoint) as Transform[];
            return positionVariants;
        }
    }
}
