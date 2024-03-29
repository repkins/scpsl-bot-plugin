using InventorySystem.Searching;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Activities
{
    internal abstract class PickupItem<C> : IActivity where C : IItemBeliefCriteria
    {
        protected abstract ItemWithinPickupDistance<C> ItemWithinPickupDistance { get; }
        protected abstract ItemInInventoryBase ItemInInventory { get; }

        public bool Condition() => _itemWithinPickupDistance.Item;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _itemWithinPickupDistance = fpcMind.ActivityEnabledBy(this, ItemWithinPickupDistance, b => b.Item);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts(this, ItemInInventory);
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
