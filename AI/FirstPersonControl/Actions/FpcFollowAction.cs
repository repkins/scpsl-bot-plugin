using Interactables;
using Interactables.Interobjects.DoorUtils;
using PlayerRoles.FirstPersonControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Actions
{
    internal class FpcFollowAction : IFpcAction
    {
        public ReferenceHub TargetToFollow { get; set; }

        public Vector3 TargetLastKnownLocation { get; set; }
        public float TargetLastTimeSeen { get; set; }

        public bool IsTargetLost { get; set; }

        public FpcFollowAction(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
            _moveAction = new FpcMoveAction(botPlayer);
            _lookAction = new FpcLookAction(botPlayer);
            _interactAction = new FpcInteractAction(botPlayer);
        }

        public void Reset()
        {
            _moveAction.Reset();
            _lookAction.Reset();
            _interactAction.Reset();

            TargetToFollow = null;
            IsTargetLost = false;
        }

        public void UpdatePlayer()
        {
            if (IsTargetLost && TargetToFollow)
            {
                IsTargetLost = false;
            }

            if (!TargetToFollow)
            {
                IsTargetLost = true;
                return;
            }

            var fpcRole = _botPlayer.FpcRole;

            var directionToTarget = (TargetToFollow.transform.position - fpcRole.FpcModule.transform.position).normalized;

            if (_botPlayer.Perception.FriendiesWithinSight.Any(p => p == TargetToFollow))
            {
                TargetLastKnownLocation = TargetToFollow.transform.position;
                TargetLastTimeSeen = Time.time;
            }

            if (Time.time - TargetLastTimeSeen > 5f)
            {
                IsTargetLost = true;
                return;
            }

            if (Vector3.Distance(TargetToFollow.transform.position, fpcRole.FpcModule.transform.position) >= 1f)
            {
                _moveAction.TargetPosition = TargetLastKnownLocation;
                _moveAction.UpdatePlayer();

                _lookAction.TargetLookDirection = _botPlayer.DesiredMoveDirection;
                _lookAction.UpdatePlayer();
            }
            else
            {
                _botPlayer.DesiredMoveDirection = Vector3.zero;
                _botPlayer.DesiredLook = Vector3.zero;
            }

            if (_botPlayer.DesiredMoveDirection != Vector3.zero
                && Vector3.Distance(fpcRole.FpcModule.transform.position, _prevPosition) < Vector3.kEpsilon)
            {
                _interactAction.UpdatePlayer();
            }

            _prevPosition = fpcRole.FpcModule.transform.position;
        }

        private FpcBotPlayer _botPlayer;
        private FpcMoveAction _moveAction;
        private FpcLookAction _lookAction;
        private FpcInteractAction _interactAction;
        private Vector3 _prevPosition;
    }
}
