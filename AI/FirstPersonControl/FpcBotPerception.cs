using Interactables.Interobjects;
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
using SCPSLBot.AI.FirstPersonControl.Perception;
using System;
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

        public List<ISense> Senses { get; } = new();

        #region Debugging
        public Dictionary<Collider, (int, string)> Layers { get; } = new Dictionary<Collider, (int, string)>();
        #endregion

        public FpcBotPerception(FpcBotPlayer fpcBotPlayer)
        {
            _fpcBotPlayer = fpcBotPlayer;
            EnemiesWithinSight = PlayersWithinSight.Where(o => o.GetFaction() != fpcBotPlayer.BotHub.PlayerHub.GetFaction())
                                                    .Where(o => o.GetFaction() != Faction.Unclassified);
            FriendiesWithinSight = PlayersWithinSight.Where(o => o.GetFaction() == fpcBotPlayer.BotHub.PlayerHub.GetFaction());

            Senses.Add(new ItemWithinSightSense(fpcBotPlayer));
        }

        public void Tick(IFpcRole fpcRole)
        {
            //var fpcTransform = fpcRole.FpcModule.transform;
            var playerHub = _fpcBotPlayer.BotHub.PlayerHub;
            var cameraTransform = _fpcBotPlayer.BotHub.PlayerHub.PlayerCameraReference;

            var prevNumOverlappingColliders = _numOverlappingColliders;
            _numOverlappingColliders = Physics.OverlapSphereNonAlloc(cameraTransform.position, 32f, _overlappingCollidersBuffer, _perceptionLayerMask);

            if (_numOverlappingColliders >= OverlappingCollidersBufferSize && _numOverlappingColliders != prevNumOverlappingColliders)
            {
                Log.Warning($"Num of overlapping colliders is equal to it's buffer size. Possible cuts.");
            }

            var overlappingColliders = _overlappingCollidersBuffer.Take(_numOverlappingColliders);

            PlayersWithinSight.Clear();
            ItemsWithinSight.Clear();
            DoorsWithinSight.Clear();
            ItemsWithinPickupDistance.Clear();

            RaycastHit[] hits;

            foreach (var collider in overlappingColliders)
            {
                if (collider.GetComponentInParent<ReferenceHub>() is ReferenceHub otherPlayer
                    && otherPlayer != _fpcBotPlayer.BotHub.PlayerHub
                    && !PlayersWithinSight.Contains(otherPlayer))
                {
                    if (IsWithinFov(cameraTransform, collider.transform)
                        && Physics.Raycast(cameraTransform.position, otherPlayer.transform.position - cameraTransform.position, out var hit)
                        && hit.collider.GetComponentInParent<ReferenceHub>() is ReferenceHub hitHub
                        && hitHub == otherPlayer)
                    {
                        PlayersWithinSight.Add(otherPlayer);
                    }
                }

                foreach (var sense in Senses)
                {
                    sense.ProcessSensibility(collider);
                }

                if (collider.GetComponentInParent<DoorVariant>() is DoorVariant door
                    && !DoorsWithinSight.Contains(door))
                {
                    if (IsWithinFov(cameraTransform, collider.transform))
                    {
                        hits = Physics.RaycastAll(cameraTransform.position, collider.bounds.center - cameraTransform.position);

                        //Log.Debug($"Overlapping within fov door {door}");

                        if (hits.Any())
                        {
                            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

                            var hit = hits.First();
                            if (hit.collider.GetComponentInParent<DoorVariant>() is DoorVariant hitDoor
                                && hitDoor == door)
                            {
                                DoorsWithinSight.Add(door);
                            }

                            //Log.Debug($"collider hit {hit.collider}");
                        }

                    }
                }
            }

            ProcessBeliefs();
        }

        private void ProcessBeliefs()
        {
            foreach (var sense in Senses)
            {
                sense.UpdateBeliefs();
            }

            ProcessDoorsWithinSight();
            ProcessItemsInInventory();
        }

        private void ProcessItemsInInventory()
        {
            var keycardInventoryBelief = _fpcBotPlayer.MindRunner.GetBelief<ItemInInventory<KeycardItem>>();
            var keycardO5InventoryBelief = _fpcBotPlayer.MindRunner.GetBelief<ItemInInventoryKeycardO5>();

            var userInventory = _fpcBotPlayer.BotHub.PlayerHub.inventory.UserInventory;

            foreach (var item in userInventory.Items.Values)
            {
                if (item is KeycardItem keycard)
                {
                    if (keycardInventoryBelief.Item is null)
                    {
                        UpdateItemInInventoryBelief(keycardInventoryBelief, keycard);
                    }
                    if (keycard.ItemTypeId == ItemType.KeycardO5 && keycardO5InventoryBelief.Item is null)
                    {
                        UpdateItemInInventoryBelief(keycardO5InventoryBelief, keycard);
                    }
                }

                HasFirearmInInventory = item is Firearm;
            }
            if (keycardInventoryBelief.Item is not null && !userInventory.Items.ContainsKey(keycardInventoryBelief.Item.ItemSerial))
            {
                UpdateItemInInventoryBelief(keycardInventoryBelief, null);
            }
            if (keycardO5InventoryBelief.Item is not null && !userInventory.Items.ContainsKey(keycardO5InventoryBelief.Item.ItemSerial))
            {
                UpdateItemInInventoryBelief(keycardO5InventoryBelief, null);
            }
        }

        private void ProcessDoorsWithinSight()
        {
            var numPryableDoors = 0u;
            var pryableWithinSightBelief = _fpcBotPlayer.MindRunner.GetBelief<DoorWithinSight<PryableDoor>>();
            foreach (var doorWithinSight in DoorsWithinSight)
            {
                if (doorWithinSight is PryableDoor gate)
                {
                    if (pryableWithinSightBelief.Door is null)
                    {
                        UpdateDoorBelief(pryableWithinSightBelief, gate);
                    }
                    numPryableDoors++;
                }
            }
            if (numPryableDoors <= 0 && pryableWithinSightBelief.Door is not null)
            {
                UpdateDoorBelief(pryableWithinSightBelief, null as PryableDoor);
            }
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
            var diff = Vector3.Normalize(targetTransform.position - transform.position);

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

        public IEnumerable<DoorVariant> GetDoorsOnPath(IEnumerable<Vector3> pathOfPoints)
        {
            var rays = pathOfPoints.Zip(pathOfPoints.Skip(1), (point, nextPoint) => new Ray(point, nextPoint - point));

            var doorsOnPath = rays
                .Select(ray => DoorsWithinSight
                    .FirstOrDefault(door => door.GetComponentsInChildren<Collider>()
                        .Any(collider => collider.Raycast(ray, out _, 1f))))
                .Where(d => d != null);

            return doorsOnPath;
        }

        private const int OverlappingCollidersBufferSize = 1000;

        private static int _numOverlappingColliders;
        private static readonly Collider[] _overlappingCollidersBuffer = new Collider[OverlappingCollidersBufferSize];

        private FpcBotPlayer _fpcBotPlayer;

        private LayerMask _perceptionLayerMask = LayerMask.GetMask("Hitbox", "Door", "InteractableNoPlayerCollision", "Glass");
    }
}
