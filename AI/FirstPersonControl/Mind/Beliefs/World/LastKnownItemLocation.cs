using InventorySystem.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.World
{
    internal class LastKnownItemLocation<T> : IBelief where T : ItemBase
    {
        public Vector3? Position { get; private set; }

        public event Action OnUpdate;

        public void Update(Vector3 value)
        {
            Position = value;
            OnUpdate?.Invoke();
        }
    }
}
