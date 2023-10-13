using Interactables;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Himself;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities
{
    internal class PickupItem<T> : IActivity where T : ItemBase
    {
        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<ItemInInventory<T>>(this);
        }

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _itemWithinSight = fpcMind.ActivityEnabledBy<ItemWithinSight<T>>(this);
            _itemWithinPickupDistance = fpcMind.ActivityEnabledBy<ItemWithinPickupDistance<T>>(this);
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
            var playerPosition = _botPlayer.FpcRole.FpcModule.Position;
            var itemPosition = _itemWithinPickupDistance.Item.transform.position;

            _botPlayer.DesiredMoveDirection = Vector3.zero;

            var directionToTarget = (itemPosition - playerPosition).normalized;

            var angleDiff = Vector3.SignedAngle(directionToTarget, _botPlayer.FpcRole.FpcModule.transform.forward, Vector3.down);
            _botPlayer.DesiredLook = new Vector3(0, angleDiff);

            if (Vector3.Distance(playerPosition, itemPosition) < 1f)
            {
                if (Physics.Raycast(_botPlayer.FpcRole.FpcModule.transform.position, _botPlayer.FpcRole.transform.forward, out var hit))
                {
                    if (hit.collider.GetComponent<InteractableCollider>() is InteractableCollider interactableCollider
                        && hit.collider.GetComponentInParent<T>() is ItemBase item)
                    {
                        Log.Debug($"Attempting to pick up item {item} by {_botPlayer}");
                    }
                }

            }
        }

        private readonly FpcBotPlayer _botPlayer;
        private ItemWithinSight<T> _itemWithinSight;
        private ItemWithinPickupDistance<T> _itemWithinPickupDistance;
    }
}
