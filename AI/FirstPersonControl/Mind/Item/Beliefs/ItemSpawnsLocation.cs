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
    internal class ItemSpawnsLocation<C> : ItemLocations<C> where C : IItemBeliefCriteria
    {
        private readonly ItemType[] spawnItemTypes;
        private readonly RoomSightSense roomSense;
        private readonly ItemsWithinSightSense itemsSightSense;

        public ItemSpawnsLocation(C criteria, ItemType[] spawnItemTypes, RoomSightSense roomSense, ItemsWithinSightSense itemsSightSense) 
            : base(criteria)
        {
            this.spawnItemTypes = spawnItemTypes;
            this.itemsSightSense = itemsSightSense;
            this.roomSense = roomSense;

            this.roomSense.OnAfterSensedForeignRooms += OnAfterSensedForeignRooms;
            this.itemsSightSense.OnAfterSensedItemsWithinSight += OnAfterSensedItemsWithinSight;
        }

        public readonly Dictionary<Vector3, float> ItemSpawnProbability = new();

        private readonly HashSet<Vector3> visitedSpawnPositions = new();
        private readonly HashSet<Vector3> absentPositions = new();

        private void OnAfterSensedItemsWithinSight()
        {
            foreach (var spawnPosition in Positions)
            {
                if (this.itemsSightSense.IsPositionWithinFov(spawnPosition)
                    //    && (!itemsSightSense.IsPositionObstructed(Position.Value) || itemsSightSense.GetDistanceToPosition(Position.Value) < 1.5f))
                    && (!itemsSightSense.IsPositionObstructed(spawnPosition)))
                {
                    this.visitedSpawnPositions.Add(spawnPosition);
                    ItemSpawnProbability.Remove(spawnPosition);

                    absentPositions.Add(spawnPosition);
                }
            }

            RemoveAllPositions(absentPositions.Remove);
        }

        private readonly List<Vector3> itemSpawnPositions = new();
        private IEnumerable<Vector3> unvisitedSpawnPositions;

        private void OnAfterSensedForeignRooms()
        {
            var roomWithin = this.roomSense.RoomWithin;
            if (roomWithin == null)
            {
                Log.Debug($"RoomSightSense.RoomWithin is null");
                return;
            }

            var foreignRooms = this.roomSense.ForeignRooms;

            itemSpawnPositions.Clear();
            foreach (var foreignRoom in foreignRooms)
            {
                itemSpawnPositions.AddRange(this.GetItemSpawnPositions(foreignRoom));
            }
            itemSpawnPositions.AddRange(this.GetItemSpawnPositions(roomWithin));

            unvisitedSpawnPositions ??= itemSpawnPositions
                .Where(spawnPosition => !this.visitedSpawnPositions.Contains(spawnPosition));

            SetPositions(unvisitedSpawnPositions);
        }

        private readonly Dictionary<RoomIdentifier, Vector3[]> roomItemSpawnPositions = new();

        private readonly List<ItemSpawnpoint> itemSpawnpoints = new();
        private IEnumerable<(Vector3 Position, float Prob)> spawnPositionsQuery;

        private Vector3[] GetItemSpawnPositions(RoomIdentifier room)
        {
            // TODO: remove when remaining nav mesh added in rooms
            if (room.Name == RoomName.LczGreenhouse || room.Name == RoomName.Hcz079)
            {
                return Array.Empty<Vector3>();
            }

            if (!this.roomItemSpawnPositions.TryGetValue(room, out var spawnPositions))
            {
                room.GetComponentsInChildren(itemSpawnpoints);

                spawnPositionsQuery ??= itemSpawnpoints
                    .Select(spawnPoint => (spawnPoint, prob: GetSpawnProbability(spawnPoint)))
                    .Where(t => t.prob > 0f)
                    .SelectMany(t => t.spawnPoint.GetPositionVariants(), 
                        (t, spawnTransform) => (spawnTransform.position, t.prob));

                spawnPositions = spawnPositionsQuery.Select(t => t.Position).ToArray();
                this.roomItemSpawnPositions.Add(room, spawnPositions);

                foreach (var (position, prob) in spawnPositionsQuery)
                {
                    this.ItemSpawnProbability.Add(position, prob);
                }
            }

            return spawnPositions;
        }

        private float GetSpawnProbability(ItemSpawnpoint spawnpoint)
        {
            var numMatchingItemTypes = this.spawnItemTypes.Count(spawnItemType => spawnpoint.InAutospawnOrAcceptedItems(spawnItemType));
            if (numMatchingItemTypes == 0)
            {
                return 0f;
            }

            var totalNumItemTypes = spawnpoint.AutospawnOrAcceptedItemsCount();
            return numMatchingItemTypes / totalNumItemTypes;
        }

        public override string ToString()
        {
            return $"{nameof(ItemSpawnsLocation<C>)}({this.Criteria}): {this.Positions.Count}";
        }
    }

    internal static class ItemSpawnpointExtensions
    {
        private static readonly FieldInfo acceptedItemsField = AccessTools.DeclaredField(typeof(ItemSpawnpoint), "_acceptedItems");
        public static bool InAutospawnOrAcceptedItems(this ItemSpawnpoint spawnpoint, ItemType itemType)
        {
            if (spawnpoint.AutospawnItem == itemType)
            {
                return true;
            }

            var spawnPointAcceptedItems = acceptedItemsField.GetValue(spawnpoint) as ItemType[];
            return spawnPointAcceptedItems.Any(i => i == itemType);
        }

        public static int AutospawnOrAcceptedItemsCount(this ItemSpawnpoint spawnpoint)
        {
            if (spawnpoint.AutospawnItem != ItemType.None)
            {
                return 1;
            }

            var acceptedItems = acceptedItemsField.GetValue(spawnpoint) as ItemType[];
            return acceptedItems!.Length;
        }

        private static readonly FieldInfo positionVariantsField = AccessTools.DeclaredField(typeof(ItemSpawnpoint), "_positionVariants");
        public static Transform[] GetPositionVariants(this ItemSpawnpoint spawnpoint)
        {
            var positionVariants = positionVariantsField.GetValue(spawnpoint) as Transform[];
            return positionVariants;
        }
    }
}
