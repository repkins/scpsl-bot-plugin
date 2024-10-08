﻿using Interactables;
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
    internal class ItemsWithinSightSense : MoveableWithinSightSense<ItemPickupBase>
    {
        public HashSet<ItemPickupBase> ItemsWithinSight => ComponentsWithinSight;

        public event Action OnBeforeSensedItemsWithinSight;
        public event Action<ItemPickupBase> OnSensedItemWithinSight;
        public event Action OnAfterSensedItemsWithinSight;

        public ItemsWithinSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        { }

        private LayerMask interactableLayerMask = LayerMask.GetMask("InteractableNoPlayerCollision");
        protected override LayerMask LayerMask => interactableLayerMask;

        public override void ProcessSightSensedItems()
        {
            ItemsWithinSight.RemoveWhere(pickup => pickup.Info.Locked);

            OnBeforeSensedItemsWithinSight?.Invoke();
            foreach (var item in ItemsWithinSight)
            {
                OnSensedItemWithinSight?.Invoke(item);
            }
            OnAfterSensedItemsWithinSight?.Invoke();
        }
    }
}
