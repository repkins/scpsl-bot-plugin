using Interactables;
using Interactables.Interobjects;
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
    internal class ItemInSightedLocker<C> : ItemLocation<C> where C : IItemBeliefCriteria
    {
        private readonly ItemType[] spawnItemTypes;
        private readonly LockersWithinSightSense lockersSightSense;

        public ItemInSightedLocker(C criteria, ItemType[] spawnItemTypes, LockersWithinSightSense lockersSightSense, FpcBotNavigator navigator) 
            : base(criteria, navigator, lockersSightSense)
        {
            this.spawnItemTypes = spawnItemTypes;
            this.lockersSightSense = lockersSightSense;

            this.lockersSightSense.OnAfterSightSensing += OnAfterSensedLockersWithinSight;
        }

        public Vector3? LockerDirection { get; private set; }
        public bool? LockerOpened { get; private set; }
        public InteractableCollider LockerDoor { get; private set; }

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

        private void OnAfterSensedLockersWithinSight()
        {
            foreach (var locker in lockersSightSense.LockersWithinSight)
            {
                //Log.Debug($"locker = {locker}");

                if (!lockerStructureTypes.Contains(locker.StructureType))
                {
                    continue;
                }

                //Log.Debug($"locker.StructureType = {locker.StructureType}");

                foreach (var chamber in locker.Chambers)
                {
                    if (!Array.Exists(chamber.AcceptableItems, type => this.spawnItemTypes.Contains(type)))
                    {
                        continue;
                    }

                    //Log.Debug($"chamber = {chamber}");

                    var itemSpawnPosition = chamber.transform.position;

                    this.itemSpawns.Add((itemSpawnPosition, chamber));
                }
            }

            if (this.AccessiblePosition.HasValue)
            {
                var itemSpawnPosition = this.AccessiblePosition.Value;

                if (this.lockersSightSense.IsPositionWithinFov(itemSpawnPosition))
                {
                    if (!lockersSightSense.IsPositionObstructed(itemSpawnPosition, out var obstruction))
                    {
                        this.visitedItemSpawnPositions.Add(itemSpawnPosition);

                        ClearPosition();
                        this.LockerDoor = null;
                        this.LockerDirection = null;
                    }
                    else if (obstruction.collider.GetComponent<InteractableCollider>() is InteractableCollider interactableObstruction
                        && interactableObstruction.Target is Locker)
                    {
                        this.LockerDoor = interactableObstruction;
                    }
                }
            }

            if (this.itemSpawns.Count > 0)
            {
                foreach (var item in itemSpawns)
                {
                    //Log.Debug($"itemSpawn = {item.Item2}");
                }
            }

            var unvisitedItemSpawn = this.itemSpawns
                .Where(itemSpawn => !this.visitedItemSpawnPositions.Contains(itemSpawn.Position))
                .Where(itemSpawn => this.IsAccessible(itemSpawn.Position))
                .Select(itemSpawn => new (Vector3, LockerChamber)?(itemSpawn))
                .FirstOrDefault();

            if (unvisitedItemSpawn.HasValue)
            {
                var (itemSpawnPosition, chamber) = unvisitedItemSpawn.Value;

                this.SetAccesablePosition(itemSpawnPosition);
                this.LockerDirection = chamber.transform.forward;
            }

            this.itemSpawns.Clear();
        }

        public override string ToString()
        {
            return $"{nameof(ItemInSightedLocker<C>)}({this.Criteria}): {this.AccessiblePosition}";
        }
    }
}
