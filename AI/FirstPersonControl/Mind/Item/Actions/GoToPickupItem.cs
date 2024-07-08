using InventorySystem.Searching;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Actions
{
    internal class GoToPickupItem<C> : GoTo<ItemSightedLocations<C>, C> where C : IItemBeliefCriteria, IEquatable<C>
    {
        public GoToPickupItem(C criteria, FpcBotPlayer botPlayer) : base(criteria, 0, botPlayer)
        {
            _botPlayer = botPlayer;
        }

        public override void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionImpacts<ItemInInventory<C>>(this, b => b.Criteria.Equals(Criteria));
        }

        public override float Weight { get; } = 1f;

        private bool isPickingUp;
        private float pickupCooldown;

        public override void Reset()
        {
            isPickingUp = false;
            pickupCooldown = 0f;
        }

        public override void Tick()
        {
            if (isPickingUp)
            {
                pickupCooldown += Time.deltaTime;
                if (pickupCooldown < 1f)
                {
                    return;
                }

                isPickingUp = false;
                pickupCooldown = 0f;
            }

            var itemPosition = location.Positions[Idx];
            var cameraPosition = _botPlayer.BotHub.PlayerHub.PlayerCameraReference.position;

            if (Vector3.Distance(itemPosition, cameraPosition) > 1.75f)
            {
                _botPlayer.MoveToPosition(itemPosition);
            }

            var cameraDirection = _botPlayer.BotHub.PlayerHub.PlayerCameraReference.forward;

            if (Vector3.Dot((itemPosition - cameraPosition).normalized, cameraDirection) <= 1f - .0001f)
            {
                _botPlayer.LookToPosition(itemPosition);
                return;
            }

            var item = _botPlayer.Perception.GetSense<ItemsWithinSightSense>().ItemsWithinSight.FirstOrDefault(i => Criteria.EvaluateItem(i) && i.Position == itemPosition);
            if (!item)
            {
                Log.Warning($"No item found at known position within sight to pickup. Moving closer.");
                _botPlayer.MoveToPosition(itemPosition);
                return;
            }

            Log.Debug($"Attempting to pick up item {item} by {_botPlayer}");

            var searchRequestMsg = new SearchRequest { Target = item };
            _botPlayer.BotHub.ConnectionToServer.Send(searchRequestMsg);

            this.isPickingUp = true;
        }

        public override string ToString()
        {
            return $"{nameof(GoToPickupItem<C>)}({this.Criteria})";
        }

        protected readonly FpcBotPlayer _botPlayer;
    }
}
