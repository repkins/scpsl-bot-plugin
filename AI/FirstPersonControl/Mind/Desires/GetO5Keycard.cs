using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Himself;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Desires
{
    internal class GetO5Keycard : IDesire
    {
        private ItemInInventory<KeycardItem> _keycardInInventory;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _keycardInInventory = fpcMind.DesireEnabledBy<ItemInInventory<KeycardItem>>(this);
        }

        public bool Condition()
        {
            return _keycardInInventory.Item?.ItemTypeId == ItemType.KeycardO5;
        }

    }
}
