﻿using InventorySystem.Searching;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities
{
    internal class PickupItem : ItemActivity, IActivity
    {
        public PickupItem(ItemType itemType, FpcBotPlayer botPlayer) : base(itemType)
        {
            this._botPlayer = botPlayer;
        }

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            this._itemWithinPickupDistance = fpcMind.ActivityEnabledBy<ItemWithinPickupDistance>(this, OfItemType, b => b.Item);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<ItemInInventory>(this, OfItemType);
        }

        public void Reset()
        {
            isPickingUp = false;
            pickupCooldown = 0f;
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

            if (Vector3.Dot((itemPosition - cameraPosition).normalized, cameraDirection) > 1f - .0001f)
            {
                Log.Debug($"Attempting to pick up item {item} by {_botPlayer}");

                var searchRequestMsg = new SearchRequest { Target = item };
                _botPlayer.BotHub.ConnectionToServer.Send(searchRequestMsg);

                this.isPickingUp = true;

                return;
            }

            _botPlayer.LookToPosition(itemPosition);
        }

        protected readonly FpcBotPlayer _botPlayer;

        private ItemWithinPickupDistance _itemWithinPickupDistance;

        private bool isPickingUp;
        private float pickupCooldown;
    }
}
