using Interactables;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Pickups;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class ItemsWithinSightSense : SightSense
    {
        public HashSet<ItemPickupBase> ItemsWithinSight { get; } = new();
        public HashSet<ItemPickupBase> ItemsWithinPickupDistance { get; } = new();

        public event Action OnBeforeSensedItemsWithinSight;
        public event Action<ItemPickupBase> OnSensedItemWithinSight;
        public event Action OnAfterSensedItemsWithinSight;

        public event Action OnBeforeSensedItemsWithinPickupDistance;
        public event Action<ItemPickupBase> OnSensedItemWithinPickupDistance;
        public event Action OnAfterSensedItemsWithinPickupDistance;

        public ItemsWithinSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        {
            _fpcBotPlayer = botPlayer;
        }

        public override void Reset()
        {
            ItemsWithinSight.Clear();
            ItemsWithinPickupDistance.Clear();
        }

        private Dictionary<Collider, ItemPickupBase> collidersOfComponent = new();

        public override void ProcessSensibility(IEnumerable<Collider> colliders)
        {
            Profiler.BeginSample($"{nameof(ItemsWithinSightSense)}.{nameof(ProcessSensibility)}");

            collidersOfComponent.Clear();
            foreach (var collider in colliders)
            {
                if ((interactableLayerMask & (1 << collider.gameObject.layer)) != 0
                    && collider.GetComponentInParent<ItemPickupBase>() is ItemPickupBase pickup && pickup && pickup.netId != 0)
                {
                    collidersOfComponent.Add(collider, pickup);
                }
            }

            var withinSight = this.GetWithinSight(collidersOfComponent.Keys);

            foreach (var collider in withinSight)
            {
                var item = collidersOfComponent[collider];

                ItemsWithinSight.Add(item);

                if (Vector3.Distance(item.transform.position, _fpcBotPlayer.CameraPosition) <= 1.75f) // TODO: constant
                {
                    ItemsWithinPickupDistance.Add(item);
                }
            }

            Profiler.EndSample();
        }

        private LayerMask interactableLayerMask = LayerMask.GetMask("InteractableNoPlayerCollision");

        public override void ProcessSightSensedItems()
        {
            OnBeforeSensedItemsWithinSight?.Invoke();
            foreach (var item in ItemsWithinSight)
            {
                OnSensedItemWithinSight?.Invoke(item);
            }
            OnAfterSensedItemsWithinSight?.Invoke();

            OnBeforeSensedItemsWithinPickupDistance?.Invoke();
            foreach (var item in ItemsWithinPickupDistance)
            {
                OnSensedItemWithinPickupDistance?.Invoke(item);
            }
            OnAfterSensedItemsWithinPickupDistance?.Invoke();
        }

        private readonly FpcBotPlayer _fpcBotPlayer;
    }
}
