using Interactables;
using Interactables.Interobjects;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Usables;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Looking;
using SCPSLBot.AI.FirstPersonControl.Mind.Activities;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Himself;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.World;
using SCPSLBot.AI.FirstPersonControl.Movement;
using System.Collections.Generic;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl
{
    internal partial class FpcBotPlayer : IBotPlayer
    {
        public FpcStandardRoleBase FpcRole { get; set; }

        public BotHub BotHub { get; }
        public FpcBotPerception Perception { get; }
        public FpcMindRunner MindRunner { get; }

        public FpcLook Look { get; }
        public FpcMove Move { get; }
        
        public FpcBotPlayer(BotHub botHub)
        {
            BotHub = botHub;
            Perception = new FpcBotPerception(this);
            MindRunner = new FpcMindRunner();

            Look = new(this);
            Move = new(this);

            MindRunner.AddBelief(new LastKnownItemLocation<KeycardItem>());
            MindRunner.AddBelief(new LastKnownItemLocation<Medkit>());
            MindRunner.AddBelief(new LastKnownItemLocation<Firearm>());

            MindRunner.AddBelief(new ItemWithinSight<KeycardPickup>());
            MindRunner.AddBelief(new ItemWithinSightMedkit());
            MindRunner.AddBelief(new ItemWithinPickupDistance<KeycardPickup>());
            MindRunner.AddBelief(new ItemInInventory<KeycardItem>());

            MindRunner.AddBelief(new DoorWithinSight<PryableDoor>());

            MindRunner.AddActivity(new GoToPickupItem<KeycardPickup>(this));
            MindRunner.AddActivity(new PickupItem<KeycardPickup, KeycardItem>(this));

            MindRunner.AddActivity(new Explore(this));

            MindRunner.SubscribeToBeliefUpdates();
        }

        public void Update()
        {
            Perception.Tick(FpcRole);
            MindRunner.Tick();
        }

        public void OnRoleChanged()
        {
            Log.Info($"Bot got FPC role assigned.");
        }

        public bool Interact(InteractableCollider interactableCollider)
        {
            if (interactableCollider == null)
            {
                if (!Physics.Raycast(FpcRole.FpcModule.transform.position, Move.DesiredDirection, out var hit))
                {
                    return false;
                }

                interactableCollider = hit.collider.GetComponent<InteractableCollider>();
            }

            if (!interactableCollider)
            {
                return false;
            }

            var interactable = interactableCollider.GetComponentInParent<IServerInteractable>();
            if (interactable == null)
            {
                return false;
            }

            var hub = BotHub.PlayerHub;
            var colliderId = interactableCollider.ColliderId;

            interactable.ServerInteract(hub, colliderId);

            return true;
        }

        #region Debug functions

        public IEnumerator<float> FindAndApproachFpcAsync()
        {
            yield break;
        }

        #endregion
    }
}
