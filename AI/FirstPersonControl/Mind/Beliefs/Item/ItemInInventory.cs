using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal class ItemInInventory : ItemInInventoryBase
    {
        public ItemType ItemType { get; }
        public ItemInInventory(ItemType itemType)
        {
            ItemType = itemType;
        }
    }
}
