using InventorySystem.Items;
using InventorySystem.Items.Pickups;
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
            if (_itemWithinPickupDistance.Item is not null)
            {
                _botPlayer.DesiredLook = Vector3.zero;
                _botPlayer.DesiredMoveDirection = Vector3.zero;
            }

            var itemPosition = _itemWithinSight.Item.transform.position;
            var playerCameraPosition = _botPlayer.FpcRole.CameraPosition;

            var hItemPosition = itemPosition;
            hItemPosition.y = playerCameraPosition.y;

            var vItemPosition = itemPosition;
            vItemPosition.x = playerCameraPosition.x;

            var hDirectionToTarget = Vector3.Normalize(hItemPosition - playerCameraPosition);
            var vDirectionToTarget = Vector3.Normalize(vItemPosition - playerCameraPosition);

            var hAngleDiff = Vector3.SignedAngle(hDirectionToTarget, _botPlayer.BotHub.PlayerHub.PlayerCameraReference.forward, Vector3.down);
            //var hAngleDiff = 0f;
            //var vAngleDiff = Vector3.SignedAngle(directionToTarget, _botPlayer.FpcRole.FpcModule.transform.forward, Vector3.left);
            var vAngleDiff = 0f;

            _botPlayer.DesiredLook = new Vector3(vAngleDiff, hAngleDiff);
            _botPlayer.DesiredMoveDirection = hDirectionToTarget;
        }

        private ItemWithinSight<T> _itemWithinSight;
        private ItemWithinPickupDistance<T> _itemWithinPickupDistance;
        private readonly FpcBotPlayer _botPlayer;
    }
}
