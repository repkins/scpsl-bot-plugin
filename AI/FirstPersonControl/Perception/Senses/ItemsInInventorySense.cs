using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Keycards;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Himself;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class ItemsInInventorySense : ISense
    {
        public bool HasFirearmInInventory { get; private set; }

        public ItemsInInventorySense(FpcBotPlayer botPlayer)
        {
            _fpcBotPlayer = botPlayer;
        }

        public void ProcessSensibility(Collider collider)
        {
        }

        public void Reset()
        {
        }

        public void UpdateBeliefs()
        {
            var keycardInventoryBelief = _fpcBotPlayer.MindRunner.GetBelief<ItemInInventory<KeycardItem>>();
            var keycardO5InventoryBelief = _fpcBotPlayer.MindRunner.GetBelief<ItemInInventoryKeycardO5>();

            var userInventory = _fpcBotPlayer.BotHub.PlayerHub.inventory.UserInventory;

            foreach (var item in userInventory.Items.Values)
            {
                if (item is KeycardItem keycard)
                {
                    if (keycardInventoryBelief.Item is null)
                    {
                        UpdateItemInInventoryBelief(keycardInventoryBelief, keycard);
                    }
                    if (keycard.ItemTypeId == ItemType.KeycardO5 && keycardO5InventoryBelief.Item is null)
                    {
                        UpdateItemInInventoryBelief(keycardO5InventoryBelief, keycard);
                    }
                }

                HasFirearmInInventory = item is Firearm;
            }
            if (keycardInventoryBelief.Item is not null && !userInventory.Items.ContainsKey(keycardInventoryBelief.Item.ItemSerial))
            {
                UpdateItemInInventoryBelief(keycardInventoryBelief, null);
            }
            if (keycardO5InventoryBelief.Item is not null && !userInventory.Items.ContainsKey(keycardO5InventoryBelief.Item.ItemSerial))
            {
                UpdateItemInInventoryBelief(keycardO5InventoryBelief, null);
            }
        }

        private static void UpdateItemInInventoryBelief<I>(ItemInInventory<I> itemBelief, I pickup) where I : ItemBase
        {
            itemBelief.Update(pickup);
            Log.Debug($"{itemBelief.GetType().Name} updated: {pickup}");
        }

        private readonly FpcBotPlayer _fpcBotPlayer;
    }
}
