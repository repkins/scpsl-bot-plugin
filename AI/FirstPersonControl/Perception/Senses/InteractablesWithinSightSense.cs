using Interactables;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

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

        private Dictionary<Collider, InteractableCollider> collidersOfComponent = new();

        public override void ProcessSensibility(IEnumerable<Collider> colliders)
        {
            Profiler.BeginSample($"{nameof(InteractablesWithinSightSense)}.{nameof(ProcessSensibility)}");

            collidersOfComponent.Clear();

            foreach (var collider in colliders)
            {
                if ((interactableLayerMask & (1 << collider.gameObject.layer)) != 0 && collider.GetComponentInParent<InteractableCollider>() is InteractableCollider interactable)
                {
                    collidersOfComponent.Add(collider, interactable);
                }
            }

            var withinSight = this.GetWithinSight(collidersOfComponent.Keys);

            foreach (var collider in withinSight)
            {
                InteractableCollidersWithinSight.Add(collidersOfComponent[collider]);
            }

            Profiler.EndSample();
        }

        private LayerMask interactableLayerMask = LayerMask.GetMask("InteractableNoPlayerCollision");

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
