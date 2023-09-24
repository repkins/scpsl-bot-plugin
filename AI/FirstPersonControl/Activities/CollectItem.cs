using Interactables;
using InventorySystem.Items;
using PlayerRoles.FirstPersonControl;
using SCPSLBot.AI.FirstPersonControl.Beliefs.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Activities
{
    internal class CollectItem<T> : IActivity where T : ItemBase
    {
        public Predicate<LastKnownItemLocation<T>> Condition = (LastKnownItemLocation<T> location) => location.Position.HasValue;

        public CollectItem(FpcBotPlayer botPlayer, LastKnownItemLocation<T> lastKnownItemLocation)
        {
            _botPlayer = botPlayer;
            _lastKnownItemLocation = lastKnownItemLocation;
        }

        public void SetImpactsBeliefs(FpcMindRunner fpcMind)
        {

        }

        public void SetEnabledByBeliefs(FpcMindRunner fpcMind)
        {
            fpcMind.ActivityEnabledBy<LastKnownItemLocation<T>>(this);
        }

        public void Tick()
        {
            var playerPosition = _botPlayer.FpcRole.FpcModule.Position;
            var itemPosition = _lastKnownItemLocation.Position.Value;

            if (Vector3.Distance(playerPosition, itemPosition) < 1f)
            {
                if (Physics.Raycast(_botPlayer.FpcRole.FpcModule.transform.position, _botPlayer.DesiredMoveDirection, out var hit))
                {
                    if (hit.collider.GetComponent<InteractableCollider>() is InteractableCollider interactableCollider
                        && hit.collider.GetComponentInParent<T>() is T item)
                    {
                        _botPlayer.Interact(interactableCollider);
                    }
                }

            }
        }

        private readonly FpcBotPlayer _botPlayer;
        private readonly LastKnownItemLocation<T> _lastKnownItemLocation;
    }
}
