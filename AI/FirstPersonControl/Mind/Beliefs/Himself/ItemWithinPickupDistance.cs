using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.PlayerLoop;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Himself
{
    internal class ItemWithinPickupDistance<T> : IBelief where T : ItemPickupBase
    {
        public event Action OnUpdate;

        public T Item { get; private set; }

        public void Update(T item)
        {
            Item = item;
            OnUpdate?.Invoke();
        }
    }
}
