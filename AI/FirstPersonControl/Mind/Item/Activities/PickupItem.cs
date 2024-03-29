using InventorySystem.Searching;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Activities
{
    internal class PickupItem<C> : IActivity where C : IItemBeliefCriteria, IEquatable<C>
    {
        public readonly C Criteria;
        public PickupItem(C criteria, FpcBotPlayer botPlayer) : this(botPlayer)
        {
            this.Criteria = criteria;
        }

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _itemWithinPickupDistance = fpcMind.ActivityEnabledBy<ItemWithinPickupDistance<C>>(this, b => b.Criteria.Equals(Criteria), b => b.Item);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<ItemInInventory<C>>(this, b => b.Criteria.Equals(Criteria));
        }

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

        public void Reset()
        {
            isPickingUp = false;
            pickupCooldown = 0f;
        }

        protected readonly FpcBotPlayer _botPlayer;

        private ItemWithinPickupDistance<C> _itemWithinPickupDistance;

        private bool isPickingUp;
        private float pickupCooldown;
    }
}
