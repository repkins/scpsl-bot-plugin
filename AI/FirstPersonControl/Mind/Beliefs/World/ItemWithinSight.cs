using InventorySystem.Items;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.World
{
    internal class ItemWithinSight<T> : IBelief where T : ItemBase
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
