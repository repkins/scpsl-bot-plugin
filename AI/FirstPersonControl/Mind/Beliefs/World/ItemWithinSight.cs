using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.World
{
    internal class ItemWithinSight<T> : IBelief where T : ItemPickupBase
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
