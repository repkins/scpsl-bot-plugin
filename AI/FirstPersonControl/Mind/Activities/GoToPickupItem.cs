using InventorySystem.Items;
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
    internal class GoToPickupItem<T> : IActivity where T : ItemBase
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
            var itemPosition = _itemWithinSight.Item.transform.position;
            var playerPosition = _botPlayer.FpcRole.FpcModule.Position;

            var directionToTarget = (itemPosition - playerPosition).normalized;

            var angleDiff = Vector3.SignedAngle(directionToTarget, _botPlayer.FpcRole.FpcModule.transform.forward, Vector3.down);
            _botPlayer.DesiredLook = new Vector3(0, angleDiff);

            _botPlayer.DesiredMoveDirection = directionToTarget;
        }

        private ItemWithinSight<T> _itemWithinSight;
        private ItemWithinPickupDistance<T> _itemWithinPickupDistance;
        private FpcBotPlayer _botPlayer;
    }
}
