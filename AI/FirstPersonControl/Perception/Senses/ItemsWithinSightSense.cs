using InventorySystem.Items.Pickups;
using System;
using System.Collections.Generic;
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

        public override void ProcessSensibility(Collider collider)
        {
            var cameraTransform = _fpcBotPlayer.BotHub.PlayerHub.PlayerCameraReference;

            if (collider.GetComponentInParent<ItemPickupBase>() is ItemPickupBase item && item
                   && !ItemsWithinSight.Contains(item))
            {
                if (IsWithinSight(collider, item))
                {
                    ItemsWithinSight.Add(item);

                    if (Vector3.Distance(item.transform.position, cameraTransform.position) <= 1.75f) // TODO: constant
                    {
                        ItemsWithinPickupDistance.Add(item);
                    }
                }
            }
        }

        public override void ProcessSensedItems()
        {
            base.ProcessSensedItems();

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
