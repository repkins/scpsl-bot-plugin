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
    internal class FpcBotFollowAction : IFpcBotAction
    {
        public ReferenceHub TargetToFollow { get; set; }

        public Vector3 TargetLastKnownLocation { get; set; }
        public float TargetLastTimeSeen { get; set; }

        public bool IsTargetLost { get; set; }

        public FpcBotFollowAction(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
        }

        public void OnEnter()
        {
            IsTargetLost = false;
            TargetLastKnownLocation = TargetToFollow.transform.position;
            TargetLastTimeSeen = Time.time;
        }

        public void UpdatePlayer(IFpcRole fpcRole)
        {
            if (!TargetToFollow || !(TargetToFollow.roleManager.CurrentRole is IFpcRole))
            {
                IsTargetLost = true;
                return;
            }

            var directionToTarget = (TargetToFollow.transform.position - fpcRole.FpcModule.transform.position).normalized;

            if (_botPlayer.Perception.FriendiesWithinSight.Any(p => p == TargetToFollow))
            {
                TargetLastKnownLocation = TargetToFollow.transform.position;
                TargetLastTimeSeen = Time.time;
            }

            if (Vector3.Distance(TargetToFollow.transform.position, fpcRole.FpcModule.transform.position) >= 1f)
            {
                _botPlayer.DesiredMoveDirection = (TargetLastKnownLocation - fpcRole.FpcModule.transform.position).normalized;

                var angleDiff = Vector3.SignedAngle(_botPlayer.DesiredMoveDirection, fpcRole.FpcModule.transform.forward, Vector3.down);
                _botPlayer.DesiredLook = new Vector3(0, angleDiff);
            }
            else
            {
                _botPlayer.DesiredMoveDirection = Vector3.zero;
                _botPlayer.DesiredLook = Vector3.zero;
            }

            if (_botPlayer.DesiredMoveDirection != Vector3.zero
                && Vector3.Distance(fpcRole.FpcModule.transform.position, _prevPosition) < Vector3.kEpsilon)
            {
                if (Physics.Raycast(fpcRole.FpcModule.transform.position, _botPlayer.DesiredMoveDirection, out var hit))
                {
                    if (hit.collider.GetComponent<InteractableCollider>() is InteractableCollider interactableCollider
                        && hit.collider.GetComponentInParent<DoorVariant>() is DoorVariant door
                        && !door.TargetState)
                    {
                        var hub = fpcRole.FpcModule.GetComponentInParent<ReferenceHub>();
                        var colliderId = interactableCollider.ColliderId;

                        door.ServerInteract(hub, colliderId);
                    }
                }
            }
            _prevPosition = fpcRole.FpcModule.transform.position;

            if (Time.time - TargetLastTimeSeen > 5f)
            {
                IsTargetLost = true;
            }
        }

        private FpcBotPlayer _botPlayer;
        private Vector3 _prevPosition;
    }
}
