using Interactables;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Actions
{
    internal class FpcInteractAction : IFpcAction
    {
        public FpcInteractAction(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
        }

        public void Reset()
        { }

        public void UpdatePlayer()
        {
            if (Physics.Raycast(_botPlayer.FpcRole.FpcModule.transform.position, _botPlayer.DesiredMoveDirection, out var hit))
            {
                if (hit.collider.GetComponent<InteractableCollider>() is InteractableCollider interactableCollider
                    && hit.collider.GetComponentInParent<IServerInteractable>() is IServerInteractable interactable)
                {
                    var hub = _botPlayer.BotHub.PlayerHub;
                    var colliderId = interactableCollider.ColliderId;

                    interactable.ServerInteract(hub, colliderId);
                }
            }
        }

        private FpcBotPlayer _botPlayer;
    }
}
