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
    internal class ItemInSightedLocker<C> : ItemLocation<C> where C : IItemBeliefCriteria
    {
        private readonly ItemType[] spawnItemTypes;
        private readonly InteractablesWithinSightSense interactablesSightSense;

        public ItemInSightedLocker(C criteria, ItemType[] spawnItemTypes, InteractablesWithinSightSense interactablesSightSense, FpcBotNavigator navigator) 
            : base(criteria, navigator, interactablesSightSense)
        {
            this.spawnItemTypes = spawnItemTypes;
            this.interactablesSightSense = interactablesSightSense;

            this.interactablesSightSense.OnSensedInteractableColliderWithinSight += ProcessSensedInteractableCollider;
            this.interactablesSightSense.OnAfterSensedInteractablesWithinSight += OnAfterSensedInteractablesWithinSight;
        }

        public Vector3? LockerDirection { get; private set; }
        public bool? LockerOpened { get; private set; }
        public InteractableCollider LockerDoor { get; private set; }

        private readonly HashSet<StructureType> lockerStructureTypes = new() { StructureType.StandardLocker, StructureType.SmallWallCabinet };

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

        private void OnAfterSensedInteractablesWithinSight()
        {
            if (this.AccessiblePosition.HasValue)
            {
                var itemSpawnPosition = this.AccessiblePosition.Value;

                if (this.interactablesSightSense.IsPositionWithinFov(itemSpawnPosition)
                    && (!interactablesSightSense.IsPositionObstructed(itemSpawnPosition)))
                {
                    this.visitedItemSpawnPositions.Add(itemSpawnPosition);

                    ClearPosition();
                    this.LockerDoor = null;
                    this.LockerDirection = null;
                    this.LockerOpened = null;
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
                var interactable = chamber.GetComponentInChildren<InteractableCollider>();

                this.SetAccesablePosition(itemSpawnPosition);
                this.LockerDirection = chamber.transform.forward;
                this.LockerOpened = chamber.IsOpen;
                this.LockerDoor = interactable;
            }

            this.itemSpawns.Clear();
        }

        public override string ToString()
        {
            return $"{nameof(ItemInSightedLocker<C>)}({this.Criteria}): {this.AccessiblePosition}";
        }
    }
}
