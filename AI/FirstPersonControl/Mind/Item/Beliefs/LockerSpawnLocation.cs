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

        private void OnAfterSightSensing()
        {
            if (this.Position.HasValue)
            {
                var spawnPosition = this.Position.Value;

                if (this.sightSense.IsPositionWithinFov(spawnPosition)
                    && (!sightSense.IsPositionObstructed(spawnPosition, out var obstruction)
                        || obstruction.GetComponentInParent<SpawnableStructure>()))
                {
                    this.visitedLockerSpawnPositions.Add(spawnPosition);
                    SetPosition(null);
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

            var unvisitedSpawnPosition = this.GetLockerSpawnPositions(roomWithin)
                .Where(spawnPosition => !this.visitedLockerSpawnPositions.Contains(spawnPosition))
                .Select(spawnPosition => new Vector3?(spawnPosition))
                .FirstOrDefault();

            if (unvisitedSpawnPosition.HasValue)
            {
                this.SetPosition(unvisitedSpawnPosition.Value);
            }
        }

        private readonly Dictionary<RoomIdentifier, Vector3[]> roomLockerSpawnPositions = new();

        private static readonly HashSet<StructureType> lockerStructureTypes = new() { StructureType.StandardLocker, StructureType.SmallWallCabinet };

        private Vector3[] GetLockerSpawnPositions(RoomIdentifier room)
        {
            if (!this.roomLockerSpawnPositions.TryGetValue(room, out var spawnPositions))
            {
                spawnPositions = room.GetComponentsInChildren<StructureSpawnpoint>()
                    .Where(spawnPoint => Array.Exists(spawnPoint.CompatibleStructures, compatableStructureType => lockerStructureTypes.Contains(compatableStructureType)))
                    .Select(spawnPoint => spawnPoint.transform.position)
                    .ToArray();
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
