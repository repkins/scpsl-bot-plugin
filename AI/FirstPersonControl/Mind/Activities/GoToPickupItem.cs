using InventorySystem.Items;
using InventorySystem.Items.Pickups;
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
    internal class GoToPickupItem<T> : IActivity where T : ItemPickupBase
    {
        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _itemWithinSight = fpcMind.ActivityEnabledBy<ItemWithinSight<T>>(this);
            _itemWithinPickupDistance = fpcMind.ActivityEnabledBy<ItemWithinPickupDistance<T>>(this);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<ItemWithinPickupDistance<T>>(this);
        }

        public Func<bool> Condition => 
            () => _itemWithinSight.Item is not null 
                && _itemWithinPickupDistance.Item is null;

        public GoToPickupItem(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
        }

        public void Tick()
        {
            _botPlayer.LookToPosition(_itemWithinSight.Item.transform.position);

            var relativePos = _itemWithinSight.Item.transform.position - _botPlayer.FpcRole.CameraPosition;
            var moveDirection = Vector3.ProjectOnPlane(relativePos, Vector3.up).normalized;
            _botPlayer.Move.DesiredDirection = moveDirection;
        }

        private ItemWithinSight<T> _itemWithinSight;
        private ItemWithinPickupDistance<T> _itemWithinPickupDistance;
        private readonly FpcBotPlayer _botPlayer;
    }
}
