using InventorySystem.Items.Pickups;
using System.Collections.Generic;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight
{
    internal abstract class MoveableWithinSightSense<TComponent> : SightSense<TComponent> where TComponent : Component
    {
        protected MoveableWithinSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        { }

        private readonly HashSet<Collider> colliders = new();
        private readonly Dictionary<Collider, ColliderData> collidersDatas = new();

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

        protected override void UpdateColliderData(Dictionary<ColliderData, TComponent> validCollidersComponents)
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
    }
}
