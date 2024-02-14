using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal abstract class ItemBase
    {
        public readonly ItemType ItemType;
        public ItemBase(ItemType itemType)
        {
            this.ItemType = itemType;
        }
    }
}
