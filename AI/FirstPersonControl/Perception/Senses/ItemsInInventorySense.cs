using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Keycards;
using PluginAPI.Core;
using PluginAPI.Core.Items;
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
            var KeycardContainmentOneInInventoryBelief = _fpcBotPlayer.MindRunner.GetBelief<KeycardContainmentOneInInventory>();

            ProcessItemBeliefs(keycardInventoryBelief, item => item is KeycardItem);
            ProcessItemBeliefs(keycardO5InventoryBelief, item => item.ItemTypeId == ItemType.KeycardO5);
            ProcessItemBeliefs(KeycardContainmentOneInInventoryBelief, item => item.Permissions.HasFlag(KeycardPermissions.ContainmentLevelOne));
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
                    if (itemBelief.Item is null)
                    {
                        UpdateItemInInventoryBelief(itemBelief, sensedItem as I);
                    }
                }

                HasFirearmInInventory = sensedItem is Firearm;
            }

            if (itemBelief.Item is not null && !sensedItems.ContainsKey(itemBelief.Item.ItemSerial))
            {
                UpdateItemInInventoryBelief(itemBelief, null as I);
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
