using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Scp914
{
    internal class ItemInIntakeChamber : IBelief
    {
        public readonly ItemType ItemType;
        public ItemInIntakeChamber(ItemType itemType)
        {
            this.ItemType = itemType;
        }

        public event Action OnUpdate;
    }
}
