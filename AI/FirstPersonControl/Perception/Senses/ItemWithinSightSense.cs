using InventorySystem.Items;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Pickups;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Himself;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class ItemWithinSightSense : SightSense, ISense
    {
        public HashSet<ItemPickupBase> ItemsWithinSight { get; } = new();
        public HashSet<ItemPickupBase> ItemsWithinPickupDistance { get; } = new();

        public ItemWithinSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        {
            _fpcBotPlayer = botPlayer;
        }

        public void ProcessSensibility(Collider collider)
        {
            var cameraTransform = _fpcBotPlayer.BotHub.PlayerHub.PlayerCameraReference;

            ItemsWithinSight.Clear();
            ItemsWithinPickupDistance.Clear();

            if (collider.GetComponentInParent<ItemPickupBase>() is ItemPickupBase item
                   && !ItemsWithinSight.Contains(item))
            {
                if (IsSensible(collider, item))
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

            var keycardSet = new (Predicate<ItemPickupBase>, ItemWithinSight<KeycardPickup>, ItemWithinPickupDistance<KeycardPickup>)[] {
                (item => item is KeycardPickup,
                    mind.GetBelief<ItemWithinSight<KeycardPickup>>(),
                    mind.GetBelief<ItemWithinPickupDistance<KeycardPickup>>()
                ),
                (item => item.Info.ItemId == ItemType.KeycardO5,
                    mind.GetBelief<ItemWithinSightKeycardO5>(),
                    mind.GetBelief<ItemWithinPickupDistanceKeycardO5>()
                ),
            };

            foreach (var (predicate, withinSight, withinPickupDistance) in keycardSet)
            {
                ProcessItemBeliefs((withinSight, withinPickupDistance), predicate);
            }

            ProcessItemPickupBelief<ItemPickupBase, ItemWithinSightMedkit>(item => item.Info.ItemId == ItemType.Medkit, ItemsWithinSight);
        }

        private void ProcessItemBeliefs<P>((ItemPickupBase<P>, ItemPickupBase<P>) beliefs, Predicate<ItemPickupBase> predicate) where P : ItemPickupBase
        {
            var (withinSight, withinPickupDistance) = beliefs;

            ProcessItemBelief(withinSight, predicate, ItemsWithinSight);
            ProcessItemBelief(withinPickupDistance, predicate, ItemsWithinPickupDistance);
        }

        private void ProcessItemPickupBelief<I, B>(Predicate<ItemPickupBase> predicate, IEnumerable<ItemPickupBase> items) where I : ItemPickupBase where B : ItemPickupBase<I>
        {
            var belief = _fpcBotPlayer.MindRunner.GetBelief<B>();
            ProcessItemBelief(belief, predicate, items);
        }

        private void ProcessItemBelief<P>(ItemPickupBase<P> belief, Predicate<ItemPickupBase> predicate, IEnumerable<ItemPickupBase> items) where P : ItemPickupBase
        {
            var numItems = 0u;

            var itemBelief = belief;
            foreach (var item in items)
            {
                if (predicate(item))
                {
                    if (itemBelief.Item is null)
                    {
                        UpdateItemBelief(itemBelief, item as P);
                    }
                    numItems++;
                }
            }

            if (numItems <= 0 && itemBelief.Item is not null)
            {
                UpdateItemBelief(itemBelief, null as P);
            }
        }

        private static void UpdateItemBelief<B, I>(B itemBelief, I pickup) where B : ItemPickupBase<I> where I : ItemPickupBase
        {
            itemBelief.Update(pickup);
            Log.Debug($"{itemBelief.GetType().Name} updated: {pickup}");
        }

        private readonly FpcBotPlayer _fpcBotPlayer;
    }
}
