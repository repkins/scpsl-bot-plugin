using Interactables;
using Scp914;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Scp914
{
    internal class Scp914Controls : Belief<Scp914KnobSetting?>
    {
        public Scp914Controls(InteractablesWithinSightSense interactablesSightSense)
        {
            interactablesSightSense.OnSensedInteractableColliderWithinSight += OnSensedInteractableColliderWithinSight;
        }

        private void OnSensedInteractableColliderWithinSight(InteractableCollider interactable)
        {
            if (interactable.Target is not Scp914Controller scp914Controller)
            {
                return;
            }

            switch ((Scp914InteractCode)interactable.ColliderId)
            {
                case Scp914InteractCode.ChangeMode:

                    Update(scp914Controller.KnobSetting);
                    this.SettingKnob = interactable;

                    break;
                case Scp914InteractCode.Activate:
                    this.StartKnob = interactable;
                    break;
            }
        }

        public Scp914KnobSetting? KnobSetting { get; private set; }
        public InteractableCollider SettingKnob { get; private set; } 
        public InteractableCollider StartKnob { get; private set; }

        private void Update(Scp914KnobSetting newSetting)
        {
            if (newSetting != this.KnobSetting)
            {
                this.KnobSetting = newSetting;
                this.InvokeOnUpdate();
            }
        }

        public override string ToString()
        {
            return $"{nameof(Scp914Controls)}: {KnobSetting}";
        }
    }
}
