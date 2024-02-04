using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Keycards;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.KeycardO5;
using System;
using System.Collections.Generic;
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
        { }

        public void Reset()
        { }

        public void UpdateBeliefs()
        {
            var keycardInventoryBelief = _fpcBotPlayer.MindRunner.GetBelief<ItemInInventory<KeycardItem>>();
            var keycardO5InventoryBelief = _fpcBotPlayer.MindRunner.GetBelief<ItemInInventoryKeycardO5>();

            ProcessItemBeliefs(keycardInventoryBelief, item => item is KeycardItem);
            ProcessItemBeliefs(keycardO5InventoryBelief, item => item.ItemTypeId == ItemType.KeycardO5);
        }

        private void ProcessItemBeliefs<P>(ItemInInventory<P> beliefs, Predicate<ItemBase> predicate) where P : ItemBase
        {
            var withinSight = beliefs;

            var userInventory = _fpcBotPlayer.BotHub.PlayerHub.inventory.UserInventory;

            ProcessItemBelief(withinSight, predicate, userInventory.Items);
        }

        private void ProcessItemBelief<P>(ItemInInventory<P> belief, Predicate<ItemBase> predicate, IDictionary<ushort, ItemBase> items) where P : ItemBase
        {
            var itemBelief = belief;
            foreach (var item in items.Values)
            {
                if (predicate(item))
                {
                    if (itemBelief.Item is null)
                    {
                        UpdateItemInInventoryBelief(itemBelief, item as P);
                    }
                }

                HasFirearmInInventory = item is Firearm;
            }

            if (itemBelief.Item is not null && !items.ContainsKey(itemBelief.Item.ItemSerial))
            {
                UpdateItemInInventoryBelief(itemBelief, null as P);
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
