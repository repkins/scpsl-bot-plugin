using Interactables.Interobjects.DoorUtils;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Door;
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
            _closedDoorWithinSight = fpcMind.ActivityEnabledBy<Scp914ChamberDoorWithinSight>(this, OfClosed, b => b.Door);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            // 914 room opened within sight
            fpcMind.ActivityImpacts<Scp914ChamberDoorWithinSight>(this, OfOpened);
        }

        public void Tick()
        {
            throw new NotImplementedException();
        }

        public void Reset() { }

        private KeycardInInventory _keycardInInventory;
        private Scp914ChamberDoorWithinSight _closedDoorWithinSight;

        private static bool OfContainmentLevelOne(KeycardInInventory b) => b.Permissions == KeycardPermissions.ContainmentLevelOne;
        private static bool OfClosed(Scp914ChamberDoorWithinSight b) => b.State == DoorState.Closed;
        private static bool OfOpened(Scp914ChamberDoorWithinSight b) => b.State == DoorState.Opened;
    }
}
