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
using Utils.NonAllocLINQ;

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

        protected override ColliderData GetEnterColliderData(Collider collider)
        {
            var colliderData = new ColliderData(collider.GetInstanceID(), collider.bounds.center);
            colliders.Add(collider);
            collidersDatas.TryAdd(collider, colliderData);
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

                var prevData = collidersDatas[collider];
                var centerPos = collider.bounds.center;
                if (prevData.Center != centerPos)
                {
                    var data = new ColliderData(prevData.InstanceId, centerPos);
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
