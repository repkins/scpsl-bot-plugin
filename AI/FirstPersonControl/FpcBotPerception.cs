﻿using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Pickups;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using PluginAPI.Core.Doors;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Himself;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.World;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl
{
    internal class FpcBotPerception
    {
        public HashSet<ReferenceHub> PlayersWithinSight { get; } = new HashSet<ReferenceHub>();
        public IEnumerable<ReferenceHub> EnemiesWithinSight { get; }
        public IEnumerable<ReferenceHub> FriendiesWithinSight { get; }

        public HashSet<ItemPickupBase> ItemsWithinSight { get; } = new ();
        public HashSet<ItemPickupBase> ItemsWithinPickupDistance { get; } = new ();
        public HashSet<DoorVariant> DoorsWithinSight { get; } = new ();

        public bool HasFirearmInInventory { get; private set; }

        #region Debugging
        public Dictionary<Collider, (int, string)> Layers { get; } = new Dictionary<Collider, (int, string)>();
        #endregion

        public FpcBotPerception(FpcBotPlayer fpcBotPlayer)
        {
            _fpcBotPlayer = fpcBotPlayer;
            EnemiesWithinSight = PlayersWithinSight.Where(o => o.GetFaction() != fpcBotPlayer.BotHub.PlayerHub.GetFaction())
                                                    .Where(o => o.GetFaction() != Faction.Unclassified);
            FriendiesWithinSight = PlayersWithinSight.Where(o => o.GetFaction() == fpcBotPlayer.BotHub.PlayerHub.GetFaction());
        }

        public void Tick(IFpcRole fpcRole)
        {
            var fpcTransform = fpcRole.FpcModule.transform;

            var prevNumOverlappingColliders = _numOverlappingColliders;
            _numOverlappingColliders = Physics.OverlapSphereNonAlloc(fpcTransform.position, 32f, _overlappingCollidersBuffer, _perceptionLayerMask);

            if (_numOverlappingColliders >= OverlappingCollidersBufferSize && _numOverlappingColliders != prevNumOverlappingColliders)
            {
                Log.Warning($"Num of overlapping colliders is equal to it's buffer size. Possible cuts.");
            }

            var overlappingColliders = _overlappingCollidersBuffer.Take(_numOverlappingColliders);

            PlayersWithinSight.Clear();
            ItemsWithinSight.Clear();
            DoorsWithinSight.Clear();
            ItemsWithinPickupDistance.Clear();

            foreach (var collider in overlappingColliders)
            {
                if (collider.GetComponentInParent<ReferenceHub>() is ReferenceHub otherPlayer
                    && otherPlayer != _fpcBotPlayer.BotHub.PlayerHub
                    && !PlayersWithinSight.Contains(otherPlayer))
                {
                    if (IsWithinFov(fpcTransform, collider.transform)
                        && Physics.Raycast(fpcTransform.position, otherPlayer.transform.position - fpcTransform.position, out var hit)
                        && hit.collider.GetComponentInParent<ReferenceHub>() is ReferenceHub hitHub
                        && hitHub == otherPlayer)
                    {
                        PlayersWithinSight.Add(otherPlayer);
                    }
                }

                if (collider.GetComponentInParent<ItemPickupBase>() is ItemPickupBase item
                    && !ItemsWithinSight.Contains(item))
                {
                    if (IsWithinFov(fpcTransform, collider.transform)
                        && Physics.Raycast(fpcTransform.position, item.transform.position - fpcTransform.position, out var hit)
                        && hit.collider.GetComponentInParent<ItemPickupBase>() is ItemPickupBase hitItem
                        && hitItem == item)
                    {
                        ItemsWithinSight.Add(item);

                        if (Vector3.Distance(item.transform.position, fpcTransform.position) <= 1f) // TODO: constant
                        {
                            ItemsWithinPickupDistance.Add(item);
                        }
                    }
                }

                if (collider.GetComponentInParent<DoorVariant>() is DoorVariant door
                    && !DoorsWithinSight.Contains(door))
                {
                    if (IsWithinFov(fpcTransform, collider.transform)
                        && Physics.Raycast(fpcTransform.position, door.transform.position - fpcTransform.position, out var hit)
                        && hit.collider.GetComponentInParent<DoorVariant>() is DoorVariant hitDoor
                        && hitDoor == door)
                    {
                        DoorsWithinSight.Add(door);
                    }
                }
            }

            ProcessBeliefs();
        }

        private void ProcessBeliefs()
        {
            ProcessItemsWithinSight();
            ProcessItemsWithinDistance();
            ProcessDoorsWithinSight();
            ProcessItemsInInventory();
        }

        private void ProcessItemsWithinSight()
        {
            var numKeycards = 0u;
            var numMedkits = 0u;

            var keycardItemBelief = _fpcBotPlayer.MindRunner.GetBelief<ItemWithinSight<KeycardPickup>>();
            var medkitItemBelief = _fpcBotPlayer.MindRunner.GetBelief<ItemWithinSightMedkit>();
            foreach (var itemWithinSight in ItemsWithinSight)
            {
                if (itemWithinSight is KeycardPickup keycard && keycardItemBelief.Item is null)
                {
                    UpdateItemBelief(keycardItemBelief, keycard);
                    numKeycards++;
                }
                if (itemWithinSight.Info.ItemId == ItemType.Medkit && medkitItemBelief.Item is null)
                {
                    UpdateItemBelief(medkitItemBelief, itemWithinSight);
                    numMedkits++;
                }
            }
            if (numKeycards <= 0 && keycardItemBelief.Item is not null)
            {
                UpdateItemBelief(keycardItemBelief, null as KeycardPickup);
            }
            if (numMedkits <= 0 && medkitItemBelief.Item is not null)
            {
                UpdateItemBelief(medkitItemBelief, null as ItemPickupBase);
            }
        }

        private void ProcessItemsWithinDistance()
        {
            var numKeycards = 0u;

            var keycardPickupBelief = _fpcBotPlayer.MindRunner.GetBelief<ItemWithinPickupDistance<KeycardPickup>>();
            foreach (var itemWithinPickup in ItemsWithinPickupDistance)
            {
                if (itemWithinPickup is KeycardPickup keycard && keycardPickupBelief.Item is null)
                {
                    UpdateItemBelief(keycardPickupBelief, keycard);
                    numKeycards++;
                }
            }
            if (numKeycards <= 0 && keycardPickupBelief.Item is not null)
            {
                UpdateItemBelief(keycardPickupBelief, null as KeycardPickup);
            }
        }

        private void ProcessItemsInInventory()
        {
            var keycardInventoryBelief = _fpcBotPlayer.MindRunner.GetBelief<ItemInInventory<KeycardItem>>();
            foreach (var item in _fpcBotPlayer.BotHub.PlayerHub.inventory.UserInventory.Items.Values)
            {
                if (item is KeycardItem keycard && keycardInventoryBelief.Item is null)
                {
                    UpdateItemInInventoryBelief(keycardInventoryBelief, keycard);
                }

                HasFirearmInInventory = item is Firearm;
            }
            if (keycardInventoryBelief.Item is not null && !_fpcBotPlayer.BotHub.PlayerHub.inventory.UserInventory.Items.ContainsKey(keycardInventoryBelief.Item.ItemSerial))
            {
                UpdateItemInInventoryBelief(keycardInventoryBelief, null);
            }
        }

        private void ProcessDoorsWithinSight()
        {
            var numPryableDoors = 0u;
            var pryableWithinSightBelief = _fpcBotPlayer.MindRunner.GetBelief<DoorWithinSight<PryableDoor>>();
            foreach (var doorWithinSight in DoorsWithinSight)
            {
                if (doorWithinSight is PryableDoor gate && pryableWithinSightBelief.Door is null)
                {
                    UpdateDoorBelief(pryableWithinSightBelief, gate);
                    numPryableDoors++;
                }
            }
            if (numPryableDoors <= 0 && pryableWithinSightBelief.Door is not null)
            {
                UpdateDoorBelief(pryableWithinSightBelief, null as PryableDoor);
            }
        }

        private static void UpdateItemBelief<T, I>(T itemBelief, I pickup) where T : ItemBase<I> where I : ItemPickupBase
        {
            itemBelief.Update(pickup);
            Log.Debug($"{itemBelief.GetType().Name} updated: {pickup}");
        }

        private static void UpdateItemInInventoryBelief<I>(ItemInInventory<I> itemBelief, I pickup) where I : ItemBase
        {
            itemBelief.Update(pickup);
            Log.Debug($"{itemBelief.GetType().Name} updated: {pickup}");
        }

        private static void UpdateDoorBelief<T, I>(T doorBelief, I door) where T : DoorBase<I> where I : DoorVariant
        {
            doorBelief.Update(door);
            Log.Debug($"{doorBelief.GetType().Name} updated: {door}");
        }

        private bool IsWithinFov(Transform transform, Transform targetTransform)
        {
            var facingDir = transform.forward;
            var diff = targetTransform.position - transform.position;

            if (Vector3.Dot(facingDir, diff) < 0)
            {
                return false;
            }

            if (Vector3.Angle(facingDir, diff) > 90)
            {
                return false;
            }

            return true;
        }

        private const int OverlappingCollidersBufferSize = 1000;

        private static int _numOverlappingColliders;
        private static readonly Collider[] _overlappingCollidersBuffer = new Collider[OverlappingCollidersBufferSize];

        private FpcBotPlayer _fpcBotPlayer;

        private LayerMask _perceptionLayerMask = LayerMask.GetMask("Hitbox", "Door", "InteractableNoPlayerCollision", "Glass");
    }
}
