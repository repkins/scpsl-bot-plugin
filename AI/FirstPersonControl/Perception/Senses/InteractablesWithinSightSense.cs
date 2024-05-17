using Interactables;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class InteractablesWithinSightSense : SightSense
    {
        public HashSet<InteractableCollider> InteractableCollidersWithinSight { get; } = new();

        public event Action OnBeforeSensedInteractablesWithinSight;
        public event Action<InteractableCollider> OnSensedInteractableColliderWithinSight;
        public event Action OnAfterSensedInteractablesWithinSight;

        public InteractablesWithinSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        {
        }

        public override void Reset()
        {
            InteractableCollidersWithinSight.Clear();
        }

        public override void ProcessSensibility(Collider collider)
        {
            if (collider.GetComponent<InteractableCollider>() is InteractableCollider interactableCollider
                && !InteractableCollidersWithinSight.Contains(interactableCollider))
            {
                if (IsWithinSight(collider, interactableCollider))
                {
                    InteractableCollidersWithinSight.Add(interactableCollider);
                }
            }
        }

        public override void ProcessSensedItems()
        {
            base.ProcessSensedItems();

            OnBeforeSensedInteractablesWithinSight?.Invoke();
            foreach (var interactableWithinSight in InteractableCollidersWithinSight)
            {
                OnSensedInteractableColliderWithinSight?.Invoke(interactableWithinSight);
            }
            OnAfterSensedInteractablesWithinSight?.Invoke();
        }
    }
}
