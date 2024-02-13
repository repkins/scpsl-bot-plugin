﻿using InventorySystem.Items.Keycards;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Desires
{
    internal class GetO5Keycard : IDesire
    {
        private ItemInInventory<KeycardItem> _keycardO5InInventory;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _keycardO5InInventory = fpcMind.DesireEnabledBy<KeycardO5InInventory>(this);
        }

        public bool Condition()
        {
            return _keycardO5InInventory.Item;
        }
    }
}
