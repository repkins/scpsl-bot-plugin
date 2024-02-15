using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class ItemsInInventorySense : ISense
    {
        public bool HasFirearmInInventory { get; private set; }

        public event Action<ItemBase> OnSensedItem;
        public event Action OnAfterSensedItems;

        public ItemsInInventorySense(FpcBotPlayer botPlayer)
        {
            _fpcBotPlayer = botPlayer;
        }

        public void ProcessSensibility(Collider collider)
        { }

        public void Reset()
        { }

        public void ProcessSensedItems()
        {
            var userInventory = _fpcBotPlayer.BotHub.PlayerHub.inventory.UserInventory;
            foreach (var item in userInventory.Items.Values)
            {
                OnSensedItem?.Invoke(item);

                HasFirearmInInventory = item is Firearm;
            }
            OnAfterSensedItems?.Invoke();
        }

        private readonly FpcBotPlayer _fpcBotPlayer;
    }
}
