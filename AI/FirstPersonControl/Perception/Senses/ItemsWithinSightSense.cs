using Interactables;
using InventorySystem.Items.Pickups;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        public override void ProcessSensibility(IEnumerable<Collider> colliders)
        {
            var collidersOfComponent = colliders
                .Select(c => (c, Component: c.GetComponentInParent<ItemPickupBase>()))
                .Where(t => t.Component is not null && t.Component && t.Component.netId != 0 && !ItemsWithinSight.Contains(t.Component));

            var withinSight = this.GetWithinSight(collidersOfComponent);

            foreach (var collider in withinSight)
            {
                var item = collider.GetComponentInParent<ItemPickupBase>();

                ItemsWithinSight.Add(item);

                if (Vector3.Distance(item.transform.position, _fpcBotPlayer.CameraPosition) <= 1.75f) // TODO: constant
                {
                    ItemsWithinPickupDistance.Add(item);
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
