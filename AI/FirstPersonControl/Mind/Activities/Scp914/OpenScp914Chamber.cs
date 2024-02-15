using Interactables.Interobjects.DoorUtils;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Door.Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Scp914
{
    internal class OpenScp914Chamber : IActivity
    {
        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _keycardInInventory = fpcMind.ActivityEnabledBy<KeycardInInventory>(this, OfContainmentLevelOne, b => b.Item);
            _closedDoorWithinSight = fpcMind.ActivityEnabledBy<ClosedScp914ChamberDoorWithinSight>(this, b => b.Door);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            // 914 room opened within sight
            fpcMind.ActivityImpacts<ClosedScp914ChamberDoorWithinSight>(this);
        }

        public void Tick()
        {
            throw new NotImplementedException();
        }

        public void Reset() { }

        private KeycardInInventory _keycardInInventory;
        private ClosedScp914ChamberDoorWithinSight _closedDoorWithinSight;

        private bool OfContainmentLevelOne(KeycardInInventory b) => b.Permissions.HasFlag(KeycardPermissions.ContainmentLevelOne);
    }
}
