using Interactables;
using Interactables.Interobjects.DoorUtils;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TestPlugin.SLBot.FirstPersonControl.Actions
{
    internal class FpcBotFollowAction : IFpcBotAction
    {
        public ReferenceHub TargetToFollow { get; set; }

        public FpcBotFollowAction(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
        }

        public void OnEnter()
        {
            TargetToFollow = null;
        }

        public void UpdatePlayer(IFpcRole fpcRole)
        {
            if (!TargetToFollow)
            {
                return;
            }

            var directionToTarget = (TargetToFollow.transform.position - fpcRole.FpcModule.transform.position).normalized;

            RaycastHit hit;
            if (Physics.Raycast(fpcRole.FpcModule.transform.position, directionToTarget, out hit))
            {
                if (hit.collider.GetComponentInParent<ReferenceHub>() is ReferenceHub hitHub 
                    && hitHub == TargetToFollow)
                {
                    TargetLastKnownLocation = hit.transform.position;
                }
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
                && Vector3.Distance(fpcRole.FpcModule.transform.position, PrevPosition) < Vector3.kEpsilon)
            {
                if (Physics.Raycast(fpcRole.FpcModule.transform.position, _botPlayer.DesiredMoveDirection, out hit))
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

            PrevPosition = fpcRole.FpcModule.transform.position;
        }

        private FpcBotPlayer _botPlayer;
        private Vector3 TargetLastKnownLocation { get; set; }
        private Vector3 PrevPosition { get; set; }
    }
}
