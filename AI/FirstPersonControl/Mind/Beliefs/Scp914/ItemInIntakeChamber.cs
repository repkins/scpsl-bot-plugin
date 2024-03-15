using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Scp914
{
    internal class ItemInIntakeChamber : IBelief
    {
        public readonly ItemType ItemType;
        public ItemInIntakeChamber(ItemType itemType)
        {
            this.ItemType = itemType;
        }

        private bool isInside;
        public bool Inside => isInside == true;
        public Vector3? DropPosition { get; private set; }

        public event Action OnUpdate;

        public void Update(bool isInside, Vector3? position = null)
        {
            this.isInside = isInside;
            this.DropPosition = position;
            this.OnUpdate?.Invoke();
        }
    }
}
