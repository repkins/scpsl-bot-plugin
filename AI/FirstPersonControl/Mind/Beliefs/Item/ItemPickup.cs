﻿using InventorySystem.Items.Pickups;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal class ItemPickup<T> : IBelief where T : ItemPickupBase
    {
        public T Item { get; private set; }

        public event Action OnUpdate;

        public void Update(T value)
        {
            Item = value;
            OnUpdate?.Invoke();
        }
    }
}