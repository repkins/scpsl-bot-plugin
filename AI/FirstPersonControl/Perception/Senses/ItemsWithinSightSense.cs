using Interactables;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Pickups;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class ItemsWithinSightSense : SightSense<ItemPickupBase>
    {
        public HashSet<ItemPickupBase> ItemsWithinSight => ComponentsWithinSight;

        public event Action OnBeforeSensedItemsWithinSight;
        public event Action<ItemPickupBase> OnSensedItemWithinSight;
        public event Action OnAfterSensedItemsWithinSight;

        public ItemsWithinSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        {
            _fpcBotPlayer = botPlayer;
        }

        private LayerMask interactableLayerMask = LayerMask.GetMask("InteractableNoPlayerCollision");
        protected override LayerMask LayerMask => interactableLayerMask;

        private readonly HashSet<Collider> colliders = new();
        private readonly Dictionary<Collider, ColliderData> collidersDatas = new();

        protected override ItemPickupBase GetComponent(ref Collider collider)
        {
            var pickup = collider.GetComponentInParent<ItemPickupBase>();
            if (pickup && pickup.netId != 0)
            {
                return pickup;
            }
            else
            {
                return null;
            }
        }

        protected override ColliderData GetEnterColliderData(Collider collider)
        {
            var colliderDataComponent = collider.GetComponent<ColliderDataComponent>();
            if (!colliderDataComponent)
            {
                colliderDataComponent = collider.gameObject.AddComponent<ColliderDataComponent>();
            }

            var colliderData = colliderDataComponent.ColliderDatas[collider];
            colliders.Add(collider);
            collidersDatas[collider] = colliderData;

            return colliderData;
        }

        protected override ColliderData GetExitColliderData(Collider collider)
        {
            colliders.Remove(collider);
            collidersDatas.Remove(collider, out var colliderData);

            return colliderData;
        }

        protected override void UpdateColliderData(Dictionary<ColliderData, ItemPickupBase> validCollidersComponents)
        {
            foreach (var collider in colliders)
            {
                if (!collider)
                {
                    continue; 
                }

                var colliderDataComponent = collider.GetComponent<ColliderDataComponent>();

                var prevData = collidersDatas[collider];
                var data = colliderDataComponent.ColliderDatas[collider];

                if (prevData.Center != data.Center)
                {
                    validCollidersComponents.Remove(prevData, out var value);
                    validCollidersComponents.Add(data, value);

                    collidersDatas[collider] = data;
                }
            }
        }

        public override void ProcessSightSensedItems()
        {
            OnBeforeSensedItemsWithinSight?.Invoke();
            foreach (var item in ItemsWithinSight)
            {
                OnSensedItemWithinSight?.Invoke(item);
            }
            OnAfterSensedItemsWithinSight?.Invoke();
        }

        private readonly FpcBotPlayer _fpcBotPlayer;
    }
}
