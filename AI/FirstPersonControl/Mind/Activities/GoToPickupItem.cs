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
            if (_itemWithinPickupDistance.Item is not null)
            {
                _botPlayer.DesiredLook = Vector3.zero;
                _botPlayer.DesiredMoveDirection = Vector3.zero;
            }

            var playerTransform = _botPlayer.FpcRole.FpcModule.transform;
            var cameraTransform = _botPlayer.BotHub.PlayerHub.PlayerCameraReference;

            var itemPosition = _itemWithinSight.Item.transform.position;
            var playerCameraPosition = _botPlayer.FpcRole.CameraPosition;

            var diff = itemPosition - playerCameraPosition;

            var hDirectionToTarget = Vector3.ProjectOnPlane(diff, Vector3.up).normalized;
            var hForward = playerTransform.forward;

            var hAngleDiff = Vector3.SignedAngle(hDirectionToTarget, hForward, Vector3.down);

            var hReverseRotation = Quaternion.AngleAxis(-hAngleDiff, Vector3.up);

            var vDirectionToTarget = Vector3.Normalize(hReverseRotation * diff);
            var vForward = cameraTransform.forward;

            var vAngleDiff = Vector3.SignedAngle(vDirectionToTarget, vForward, cameraTransform.right);

            _botPlayer.DesiredLook = new Vector3(vAngleDiff, hAngleDiff);
        }

        private ItemWithinSight<T> _itemWithinSight;
        private ItemWithinPickupDistance<T> _itemWithinPickupDistance;
        private readonly FpcBotPlayer _botPlayer;
    }
}
