using Interactables;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public override void ProcessSensibility(IEnumerable<Collider> colliders)
        {
            var collidersOfComponent = colliders
                .Select(c => (c, Component: c.GetComponentInParent<InteractableCollider>()))
                .Where(t => t.Component is not null && !InteractableCollidersWithinSight.Contains(t.Component));

            var withinSight = this.GetWithinSight(collidersOfComponent);

            foreach (var collider in withinSight)
            {
                InteractableCollidersWithinSight.Add(collider.GetComponentInParent<InteractableCollider>());
            }
        }

        public override void ProcessSightSensedItems()
        {
            OnBeforeSensedInteractablesWithinSight?.Invoke();
            foreach (var interactableWithinSight in InteractableCollidersWithinSight)
            {
                OnSensedInteractableColliderWithinSight?.Invoke(interactableWithinSight);
            }
            OnAfterSensedInteractablesWithinSight?.Invoke();
        }
    }
}
