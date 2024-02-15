using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;
using System;
using System.Collections.Generic;
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

        public void UpdateBeliefs()
        {
            var userInventory = _fpcBotPlayer.BotHub.PlayerHub.inventory.UserInventory;
            foreach (var item in userInventory.Items.Values)
            {
                OnSensedItem?.Invoke(item);
            }
            OnAfterSensedItems?.Invoke();

            var keycardInInventoryBeliefs = _fpcBotPlayer.MindRunner.GetBeliefs<KeycardInInventory>();

            foreach (var keycardInInventoryBelief in keycardInInventoryBeliefs)
            {
                ProcessItemBeliefs(keycardInInventoryBelief, item => item.Permissions.HasFlag(keycardInInventoryBelief.Permissions));
            }
        }

        private void ProcessItemBeliefs<I>(ItemInInventory<I> beliefs, Predicate<I> predicate) where I : ItemBase
        {
            var inInventoryBelief = beliefs;

            var userInventory = _fpcBotPlayer.BotHub.PlayerHub.inventory.UserInventory;

            ProcessItemBelief(inInventoryBelief, predicate, userInventory.Items);
        }

        private void ProcessItemBelief<I>(ItemInInventory<I> belief, Predicate<I> predicate, IDictionary<ushort, ItemBase> sensedItems) where I : ItemBase
        {
            var itemBelief = belief;
            foreach (var sensedItem in sensedItems.Values)
            {
                if (sensedItem is I itemOf && predicate(itemOf))
                {
                    if (!itemBelief.Item)
                    {
                        UpdateItemInInventoryBelief(itemBelief, sensedItem as I);
                    }
                }

                HasFirearmInInventory = sensedItem is Firearm;
            }

            if (itemBelief.Item && !sensedItems.ContainsKey(itemBelief.Item.ItemSerial))
            {
                UpdateItemInInventoryBelief(itemBelief, null);
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
