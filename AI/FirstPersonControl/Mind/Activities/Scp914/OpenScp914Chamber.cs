using Interactables.Interobjects.DoorUtils;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Scp914;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Scp914
{
    internal class OpenScp914Chamber : IActivity
    {
        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _keycardInInventory = fpcMind.ActivityEnabledBy<KeycardInInventory>(this, OfContainmentLevelOne, b => b.Item);
            _closedDoorWithinSight = fpcMind.ActivityEnabledBy<Scp914ChamberDoor>(this, OfClosed, b => b.Door);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            // 914 room opened within sight
            fpcMind.ActivityImpacts<Scp914ChamberDoor>(this, OfOpened);
        }

        public void Tick()
        {
            throw new NotImplementedException();
        }

        public void Reset() { }

        private KeycardInInventory _keycardInInventory;
        private Scp914ChamberDoor _closedDoorWithinSight;

        private static bool OfContainmentLevelOne(KeycardInInventory b) => b.Permissions == KeycardPermissions.ContainmentLevelOne;
        private static bool OfClosed(Scp914ChamberDoor b) => b.State == DoorState.Closed;
        private static bool OfOpened(Scp914ChamberDoor b) => b.State == DoorState.Opened;
    }
}
