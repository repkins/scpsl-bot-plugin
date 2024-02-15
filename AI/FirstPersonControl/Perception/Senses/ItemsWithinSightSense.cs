using Interactables.Interobjects.DoorUtils;
using InventorySystem;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Pickups;
using PluginAPI.Core;
using PluginAPI.Core.Items;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class ItemsWithinSightSense : SightSense, ISense
    {
        public HashSet<ItemPickupBase> ItemsWithinSight { get; } = new();
        public HashSet<ItemPickupBase> ItemsWithinPickupDistance { get; } = new();

        public ItemsWithinSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        {
            _fpcBotPlayer = botPlayer;
        }

        public void Reset()
        {
            ItemsWithinSight.Clear();
            ItemsWithinPickupDistance.Clear();
        }

        public void ProcessSensibility(Collider collider)
        {
            var cameraTransform = _fpcBotPlayer.BotHub.PlayerHub.PlayerCameraReference;

            if (collider.GetComponentInParent<ItemPickupBase>() is ItemPickupBase item && item
                   && !ItemsWithinSight.Contains(item))
            {
                if (IsWithinSight(collider, item))
                {
                    ItemsWithinSight.Add(item);

                    if (Vector3.Distance(item.transform.position, cameraTransform.position) <= 1.75f) // TODO: constant
                    {
                        ItemsWithinPickupDistance.Add(item);
                    }
                }
            }
        }

        public void UpdateBeliefs()
        {
            var mind = _fpcBotPlayer.MindRunner;

            var itemWithinSightBeliefs = mind.GetBeliefs<ItemWithinSight>();
            var itemWithinPickupDistanceBeliefs = mind.GetBeliefs<ItemWithinPickupDistance>();
            foreach (var itemWithinSightBelief in itemWithinSightBeliefs)
            {
                ProcessItemBelief(itemWithinSightBelief, item => item.Info.ItemId == itemWithinSightBelief.ItemType, ItemsWithinSight);
            }
            foreach (var itemWithinPickupDistanceBelief in itemWithinPickupDistanceBeliefs)
            {                
                ProcessItemBelief(itemWithinPickupDistanceBelief, item => item.Info.ItemId == itemWithinPickupDistanceBelief.ItemType, ItemsWithinPickupDistance);
            }

            var keycardWithinSightBeliefs = mind.GetBeliefs<KeycardWithinSight>();
            var keycardWithinPickupDistanceBeliefs = mind.GetBeliefs<KeycardWithinPickupDistance>();
            foreach (var keycardWithinSightBelief in keycardWithinSightBeliefs)
            {
                ProcessItemBelief(keycardWithinSightBelief, item => InventoryItemLoader.TryGetItem<KeycardItem>(item.Info.ItemId, out var keycard) 
                    && keycard.Permissions.HasFlag(keycardWithinSightBelief.Permissions), ItemsWithinSight);
            }
            foreach (var keycardWithinPickupDistanceBelief in keycardWithinPickupDistanceBeliefs)
            {
                ProcessItemBelief(keycardWithinPickupDistanceBelief, item => InventoryItemLoader.TryGetItem<KeycardItem>(item.Info.ItemId, out var keycard)
                    && keycard.Permissions.HasFlag(keycardWithinPickupDistanceBelief.Permissions), ItemsWithinPickupDistance);
            }

            var medkitWithinSight = mind.GetBelief<ItemWithinSight>(b => b.ItemType == ItemType.Medkit);
            ProcessItemBelief(medkitWithinSight, b => b.Info.ItemId == ItemType.Medkit, ItemsWithinSight);
        }

        private void ProcessItemBelief<P>(ItemPickup<P> belief, Predicate<ItemPickupBase> predicate, IEnumerable<ItemPickupBase> items) where P : ItemPickupBase
        {
            var numItems = 0u;

            var itemBelief = belief;
            foreach (var item in items)
            {
                if (predicate(item))
                {
                    if (!itemBelief.Item)
                    {
                        UpdateItemBelief(itemBelief, item as P);
                    }
                    numItems++;
                }
            }

            if (numItems <= 0 && itemBelief.Item)
            {
                UpdateItemBelief(itemBelief, null as P);
            }
        }

        private static void UpdateItemBelief<B, I>(B itemBelief, I pickup) where B : ItemPickup<I> where I : ItemPickupBase
        {
            itemBelief.Update(pickup);
            Log.Debug($"{itemBelief.GetType().Name} updated: {pickup}");
        }

        private readonly FpcBotPlayer _fpcBotPlayer;
    }
}
