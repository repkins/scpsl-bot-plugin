using MapGeneration;
using MapGeneration.Distributors;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs
{
    internal class LockerSpawnLocation : IBelief
    {
        private readonly RoomSightSense roomSense;
        private readonly SightSense sightSense;

        public LockerSpawnLocation(RoomSightSense roomSense)
        {
            this.roomSense = roomSense;
            this.sightSense = roomSense;

            this.roomSense.OnAfterSensedForeignRooms += OnAfterSensedForeignRooms;
            this.sightSense.OnAfterSightSensing += OnAfterSightSensing;
        }

        private readonly HashSet<Vector3> visitedLockerSpawnPositions = new();

        private readonly Stopwatch stopwatch = new();

        private void OnAfterSightSensing()
        {
            if (this.Position.HasValue)
            {
                var spawnPosition = this.Position.Value;

                if (this.sightSense.IsPositionWithinFov(spawnPosition)
                    && (!sightSense.IsPositionObstructed(spawnPosition, out var obstruction)
                        || Vector3.Distance(obstruction.point, spawnPosition) < 1f)
                    && !this.stopwatch.IsRunning)
                {
                    this.stopwatch.Restart();
                }

                if (this.stopwatch.IsRunning && this.stopwatch.ElapsedMilliseconds > 500)
                {
                    this.stopwatch.Stop();

                    this.visitedLockerSpawnPositions.Add(spawnPosition);
                    SetPosition(null);
                }
            }
        }

        private readonly List<Vector3> spawnPositions = new();
        private IEnumerable<Vector3?> unvisitedSpawnPositions;

        private void OnAfterSensedForeignRooms()
        {
            var roomWithin = this.roomSense.RoomWithin;
            if (roomWithin == null)
            {
                Log.Debug($"RoomSightSense.RoomWithin is null");
                return;
            }

            var foreignRooms = this.roomSense.ForeignRooms;

            spawnPositions.Clear();
            foreach (var foreignRoom in foreignRooms)
            {
                spawnPositions.AddRange(this.GetLockerSpawnPositions(foreignRoom));
            }
            spawnPositions.AddRange(this.GetLockerSpawnPositions(roomWithin));

            unvisitedSpawnPositions ??= spawnPositions
                .Where(spawnPosition => !this.visitedLockerSpawnPositions.Contains(spawnPosition))
                .Select(spawnPosition => new Vector3?(spawnPosition));

            var unvisitedSpawnPosition = unvisitedSpawnPositions.FirstOrDefault();
            if (unvisitedSpawnPosition.HasValue)
            {
                this.SetPosition(unvisitedSpawnPosition.Value);
            }
        }

        private readonly Dictionary<RoomIdentifier, Vector3[]> roomLockerSpawnPositions = new();

        private static readonly HashSet<StructureType> lockerStructureTypes = new() { StructureType.StandardLocker, StructureType.SmallWallCabinet };

        private readonly List<StructureSpawnpoint> structureSpawnpoints = new();
        private IEnumerable<Vector3> spawnPositionsQuery;

        private Vector3[] GetLockerSpawnPositions(RoomIdentifier room)
        {
            if (!this.roomLockerSpawnPositions.TryGetValue(room, out var spawnPositions))
            {
                room.GetComponentsInChildren(structureSpawnpoints);

                spawnPositionsQuery ??= structureSpawnpoints
                    .Where(spawnPoint => Array.Exists(spawnPoint.CompatibleStructures, compatableStructureType => lockerStructureTypes.Contains(compatableStructureType)))
                    .Select(spawnPoint => spawnPoint.transform.position);

                spawnPositions = spawnPositionsQuery.ToArray();

                this.roomLockerSpawnPositions.Add(room, spawnPositions);
            }

            return spawnPositions;
        }

        public override string ToString()
        {
            return $"{nameof(LockerSpawnLocation)}: {this.Position}";
        }

        public Vector3? Position { get; private set; }
        public event Action OnUpdate;

        private void SetPosition(Vector3? newPosition)
        {
            if (newPosition != this.Position)
            {
                this.Position = newPosition;
                this.OnUpdate?.Invoke();
            }
        }
    }
}
