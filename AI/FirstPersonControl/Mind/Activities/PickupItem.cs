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

        public Func<bool> Condition => 
            () => _itemWithinSight.Item is not null
                && _itemWithinPickupDistance.Item is not null;

        public PickupItem(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
        }

        public void Tick()
        {
            if (isPickingUp)
            {
                return;
            }

            var playerPosition = _botPlayer.FpcRole.FpcModule.Position;
            var itemPosition = _itemWithinPickupDistance.Item.transform.position;

            _botPlayer.DesiredMoveDirection = Vector3.zero;

            var directionToTarget = (itemPosition - playerPosition).normalized;

            var angleDiff = Vector3.SignedAngle(directionToTarget, _botPlayer.FpcRole.FpcModule.transform.forward, Vector3.down);
            _botPlayer.DesiredLookAngles = new Vector3(0, angleDiff);

            if (Vector3.Distance(playerPosition, itemPosition) < 1f)
            {
                if (Physics.Raycast(_botPlayer.FpcRole.FpcModule.transform.position, _botPlayer.FpcRole.transform.forward, out var hit))
                {
                    if (hit.collider.GetComponent<InteractableCollider>() is not null
                        && hit.collider.GetComponentInParent<P>() is P item)
                    {
                        Log.Debug($"Attempting to pick up item {item} by {_botPlayer}");

                        var searchRequestMsg = new SearchRequest { Target = item };
                        _botPlayer.BotHub.ConnectionToServer.Send(searchRequestMsg);

                        this.isPickingUp = true;
                    }
                }
            }
        }

        private readonly FpcBotPlayer _botPlayer;

        private ItemWithinSight<P> _itemWithinSight;
        private ItemWithinPickupDistance<P> _itemWithinPickupDistance;

        private bool isPickingUp;
    }
}
