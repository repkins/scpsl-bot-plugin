using Interactables;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
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
    [ActivityEnabledBy<LastKnownItemLocation<ItemBase>>]
    internal class CollectItem<T> : IActivity where T : ItemBase
    {
        public void SetImpactsBeliefs(FpcMindRunner fpcMind)
        {

        }

        public void SetEnabledByBeliefs(FpcMindRunner fpcMind)
        {
            _lastKnownItemLocation = fpcMind.ActivityEnabledBy<LastKnownItemLocation<T>>(this);
        }

        public bool Condition => _lastKnownItemLocation.Position.HasValue;

        public CollectItem(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
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
                        && hit.collider.GetComponentInParent<T>() is ItemPickupBase item)
                    {
                        _botPlayer.Interact(interactableCollider);
                    }
                }

            }
        }

        private readonly FpcBotPlayer _botPlayer;
        private LastKnownItemLocation<T> _lastKnownItemLocation;
    }
}
