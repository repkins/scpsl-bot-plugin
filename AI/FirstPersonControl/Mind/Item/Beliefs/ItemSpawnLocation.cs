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
        public ItemSpawnLocation(C criteria, ItemType[] spawnItemTypes, RoomSightSense roomSense, ItemsWithinSightSense itemsSightSense, FpcBotNavigator navigator) 
            : this(spawnItemTypes, roomSense, itemsSightSense, navigator)
        {
            this.Criteria = criteria;
        }

        private readonly ItemType[] spawnItemTypes;
        private readonly RoomSightSense roomSense;
        private readonly ItemsWithinSightSense itemsSightSense;
        private readonly FpcBotNavigator navigator;

        private ItemSpawnLocation(ItemType[] spawnItemTypes, RoomSightSense roomSense, ItemsWithinSightSense itemsSightSense, FpcBotNavigator navigator)
        {
            this.spawnItemTypes = spawnItemTypes;
            this.roomSense = roomSense;
            this.itemsSightSense = itemsSightSense;
            this.navigator = navigator;

            this.roomSense.OnAfterSensedForeignRooms += OnAfterSensedForeignRooms;
            this.itemsSightSense.OnAfterSensedItemsWithinSight += OnAfterSensedItemsWithinSight;
        }

        private readonly HashSet<Vector3> visitedSpawnPositions = new();
        private readonly HashSet<Vector3> inaccesableSpawnPositions = new();

        private void OnAfterSensedItemsWithinSight()
        {
            if (this.Position.HasValue)
            {
                var spawnPosition = this.Position.Value;

                if (!this.IsAccessable(spawnPosition))
                {
                    this.inaccesableSpawnPositions.Add(spawnPosition);
                    Update(null);
                    return;
                }

                if (this.itemsSightSense.IsPositionWithinFov(spawnPosition)
                //    && (!itemsSightSense.IsPositionObstructed(Position.Value) || itemsSightSense.GetDistanceToPosition(Position.Value) < 1.5f))
                    && (!itemsSightSense.IsPositionObstructed(spawnPosition)))
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
                    .Where(spawnPosition => this.IsAccessable(spawnPosition))
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
            // TODO: remove when remaining nav mesh added in room
            if (room.Name == RoomName.LczGreenhouse)
            {
                return Array.Empty<Vector3>();
            }

            if (!this.roomItemSpawnPositions.TryGetValue(room, out var spawnPositions))
            {
                spawnPositions = room.GetComponentsInChildren<ItemSpawnpoint>()
                    .Where(spawnPoint => this.spawnItemTypes.Any(spawnItemType => spawnPoint.InAcceptedItems(spawnItemType)))
                    .SelectMany(spawnPoint => spawnPoint.GetPositionVariants())
                    .Select(positionVariant => positionVariant.position)
                    .ToArray();
            }
            return spawnPositions;
        }

        private bool IsAccessable(Vector3 spawnPosition)
        {
            var pathOfPoints = this.navigator.PointsPath;

            if (pathOfPoints.Last() != spawnPosition)
            {
                return !this.inaccesableSpawnPositions.Contains(spawnPosition);
            }

            var pathSegments = pathOfPoints.Zip(pathOfPoints.Skip(1), (point, nextPoint) => (point, nextPoint));

            var hits = pathSegments.Select(segment => (isHit: Physics.Linecast(segment.point, segment.nextPoint, out var hit), hit))
                .Where(t => t.isHit);

            if (hits.All(t => this.Criteria.CanOvercome(t.hit.collider)))
            {
                this.inaccesableSpawnPositions.Remove(spawnPosition);

                return true;
            }

            Log.Debug($"{this}: cannot overcome");

            return false;
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

        private static readonly FieldInfo positionVariantsField = AccessTools.DeclaredField(typeof(ItemSpawnpoint), "_positionVariants");
        public static Transform[] GetPositionVariants(this ItemSpawnpoint spawnpoint)
        {
            var positionVariants = positionVariantsField.GetValue(spawnpoint) as Transform[];
            return positionVariants;
        }
    }
}
