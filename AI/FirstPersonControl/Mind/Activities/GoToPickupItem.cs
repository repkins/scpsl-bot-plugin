﻿using InventorySystem.Items;
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
        public virtual void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _itemWithinSight = fpcMind.ActivityEnabledBy<ItemWithinSight<T>>(this);
        }

        public virtual void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<ItemWithinPickupDistance<T>>(this);
        }

        public bool Condition() => _itemWithinSight.Item;

        public GoToPickupItem(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
        }

        public void Tick()
        {
            _botPlayer.LookToPosition(_itemWithinSight.Item.transform.position);

            var relativePos = _itemWithinSight.Item.transform.position - _botPlayer.FpcRole.CameraPosition;
            var moveDirection = Vector3.ProjectOnPlane(relativePos, Vector3.up).normalized;
            _botPlayer.Move.DesiredLocalDirection = _botPlayer.FpcRole.FpcModule.transform.InverseTransformDirection(moveDirection);
        }

        public void Reset() { }

        protected ItemWithinSight<T> _itemWithinSight;
        private readonly FpcBotPlayer _botPlayer;
    }
}
