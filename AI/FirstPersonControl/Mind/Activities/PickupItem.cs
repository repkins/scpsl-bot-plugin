using Interactables;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using InventorySystem.Searching;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Himself;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.World;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities
{
    internal class PickupItem<P, I> : IActivity where P : ItemPickupBase 
                                                where I : ItemBase
    {
        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<ItemInInventory<I>>(this);
        }

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _itemWithinSight = fpcMind.ActivityEnabledBy<ItemWithinSight<P>>(this);
            _itemWithinPickupDistance = fpcMind.ActivityEnabledBy<ItemWithinPickupDistance<P>>(this);
        }

        public bool Condition() => _itemWithinSight.Item
                                && _itemWithinPickupDistance.Item;

        public PickupItem(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
        }

        public void Tick()
        {
            if (isPickingUp)
            {
                pickupCooldown += Time.deltaTime;
                if (pickupCooldown < 1f)
                {
                    return;
                }

                pickupCooldown = 0f;
                isPickingUp = false;
            }

            var playerPosition = _botPlayer.FpcRole.FpcModule.Position;
            var cameraPosition = _botPlayer.BotHub.PlayerHub.PlayerCameraReference.position;
            var itemPosition = _itemWithinPickupDistance.Item.transform.position;
            var cameraDirection = _botPlayer.BotHub.PlayerHub.PlayerCameraReference.forward;

            var item = _itemWithinPickupDistance.Item;

            if (Vector3.Dot(itemPosition - cameraPosition, cameraDirection) >= 1f - Mathf.Epsilon)
            {
                Log.Debug($"Attempting to pick up item {item} by {_botPlayer}");

                var searchRequestMsg = new SearchRequest { Target = item };
                _botPlayer.BotHub.ConnectionToServer.Send(searchRequestMsg);

                this.isPickingUp = true;

                return;
            }

            _botPlayer.LookToPosition(itemPosition);
        }

        private readonly FpcBotPlayer _botPlayer;

        private ItemWithinSight<P> _itemWithinSight;
        private ItemWithinPickupDistance<P> _itemWithinPickupDistance;

        private bool isPickingUp;
        private float pickupCooldown = 0f;
    }
}
