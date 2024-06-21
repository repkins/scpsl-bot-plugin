using Interactables;
using MapGeneration.Distributors;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs
{
    internal class ItemSpawnsInSightedLocker<C> : ItemLocations<C> where C : IItemBeliefCriteria
    {
        private readonly ItemType[] spawnItemTypes;
        private readonly LockersWithinSightSense lockersSightSense;

        public ItemSpawnsInSightedLocker(C criteria, ItemType[] spawnItemTypes, LockersWithinSightSense lockersSightSense) 
            : base(criteria)
        {
            this.spawnItemTypes = spawnItemTypes;
            this.lockersSightSense = lockersSightSense;

            this.lockersSightSense.OnAfterSightSensing += OnAfterSensedLockersWithinSight;
        }

        public Dictionary<Vector3, Vector3> LockerDirections { get; } = new();
        public Dictionary<Vector3, InteractableCollider> LockerDoors { get; } = new();

        private static readonly HashSet<StructureType> lockerStructureTypes = new() { StructureType.StandardLocker, StructureType.SmallWallCabinet };

        private readonly HashSet<(Vector3 Position, LockerChamber)> itemSpawns = new();
        private readonly HashSet<Vector3> visitedItemSpawnPositions = new();

        private void ProcessSensedInteractableCollider(InteractableCollider interactable)
        {
            if (interactable.Target is not Locker locker)
            {
                return;
            }


            if (!lockerStructureTypes.Contains(locker.StructureType))
            {
                return;
            }

            var chamber = locker.Chambers[interactable.ColliderId];
            if (!Array.Exists(chamber.AcceptableItems, type => this.spawnItemTypes.Contains(type)))
            {
                return;
            }

            var itemSpawnPosition = chamber.transform.position;

            this.itemSpawns.Add((itemSpawnPosition, chamber));
        }

        private readonly HashSet<Vector3> absentPositions = new();

        private void OnAfterSensedLockersWithinSight()
        {
            foreach (var locker in lockersSightSense.LockersWithinSight)
            {
                if (!lockerStructureTypes.Contains(locker.StructureType))
                {
                    continue;
                }

                foreach (var chamber in locker.Chambers)
                {
                    if (!Array.Exists(chamber.AcceptableItems, type => this.spawnItemTypes.Contains(type)))
                    {
                        continue;
                    }

                    var itemSpawnPosition = chamber.transform.position;

                    this.itemSpawns.Add((itemSpawnPosition, chamber));
                }
            }

            foreach (var itemSpawnPosition in this.Positions)
            {
                if (this.lockersSightSense.IsPositionWithinFov(itemSpawnPosition))
                {
                    if (!lockersSightSense.IsPositionObstructed(itemSpawnPosition, out var obstruction))
                    {
                        this.visitedItemSpawnPositions.Add(itemSpawnPosition);
                        absentPositions.Add(itemSpawnPosition);

                        this.LockerDoors.Remove(itemSpawnPosition);
                        this.LockerDirections.Remove(itemSpawnPosition);
                    }
                    else if (obstruction.collider.GetComponent<InteractableCollider>() is InteractableCollider interactableObstruction
                        && interactableObstruction.Target is Locker)
                    {
                        this.LockerDoors[itemSpawnPosition] = interactableObstruction;
                    }
                }
            }

            RemoveAllPositions(absentPositions.Remove);

            var unvisitedItemSpawns = this.itemSpawns
                .Where(itemSpawn => !this.visitedItemSpawnPositions.Contains(itemSpawn.Position));

            SetPositions(unvisitedItemSpawns.Select(s => s.Position));
            foreach (var (position, chamber) in unvisitedItemSpawns)
            {
                this.LockerDirections[position] = chamber.transform.forward;
            }

            this.itemSpawns.Clear();
        }

        public override string ToString()
        {
            return $"{nameof(ItemSpawnsInSightedLocker<C>)}({this.Criteria}): {this.Positions.Count}";
        }
    }
}
